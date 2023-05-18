// $fa=3; $fs=0.1; // fine detail
$fa=6; $fs=0.2; // coarse detail

hulldia=9.0;
height=48.0;
curvelen=15.0;

raptordia=2.0;
raptorspace=2.4;
raptorhole=0.9*3*raptorspace;
raptorheight=2.0;
raptorstart=-1.5;

// square([2.1,hulldia],center=true); // dividing line for 3-on

module roundcyl(dia,height) {
	hull() {
		sphere(d=dia);
		translate([0,0,height])
			sphere(d=dia);
	}
}

module leading_edge(inset,height) {
	hull() {
		d=hulldia-2*inset;
		cylinder(d=d,h=height-curvelen);
		
		translate([0,0,height-curvelen])
		scale([d,d,2*curvelen])
		difference() {
			sphere(r=0.5,$fs=$fs/4);
			translate([0,0,-1.1])
				cube([2,2,2],center=true);
		}
	}
}

module interior_spaces() {
	tank_wall=0.3;
	tankdia=hulldia-2.0*tank_wall;
	tank_start=3+raptorheight;
	tank_scale=0.5; // rounding on tank domes
	low_tank=15;
	intertank=tank_wall;
	high_tank=8;
	high_tank_shift=low_tank+intertank/tank_scale+tankdia;
	
	translate([0,0,tank_start])
	scale([1,1,tank_scale])
	{
		roundcyl(tankdia,low_tank);
		translate([0,0,high_tank_shift])
		roundcyl(tankdia,high_tank);
	}
	
	cabin_wall=0.5;
	cabin_start=tank_start+(high_tank_shift+high_tank+tankdia/2)*tank_scale;
	translate([0,0,cabin_start+cabin_wall])
	difference() {
		leading_edge(cabin_wall,height-cabin_start-2*cabin_wall);
		for (angle=[0:45:359])
			rotate([0,0,angle])
				translate([1.5,0,0])
					cube([10,0.1,15.7]);
		for (deck=[1:5])
			translate([0,0,deck*3])
			difference() {
				cube([hulldia,hulldia,0.2],center=true);
				cylinder(d=2,h=1,center=true);
			}
	}
	
	
	// Main front window
	difference() {
		translate([-hulldia/2,2,height-7])
			cube([hulldia,hulldia,height]);
		
		for (stringer=[-4:1:+4])
			translate([stringer,0,0])
				cube([0.1,hulldia,height]);
	}
	
	// cube([100,100,100]); // Cutaway cube
}

module starship() {
	difference() {
		union() {
			// Exterior cylinder
			difference() {
				leading_edge(0.0,height);
				
				translate([0,0,-0.01])
				cylinder(d1=hulldia,d2=raptorhole,h=raptorstart+raptorheight*0.8);
				raptor_centers() 
					cylinder(d=1.5,h=raptorheight+0.5);
			}
			
			// Front control surfaces
			for (canard=[-1,+1])
				hull() {
					translate([0,0,height-6]) 
					{
						roundcyl(1.0,5.0); // inside
						translate([canard*7.0,0,-5])
							roundcyl(0.5,2.5); // outside
					}
				}
			
			// Rear surfaces
			for (fin=[-120,0,+120])
				rotate([0,0,fin])
				hull() {
					translate([0,0,-2.0]) 
					{
						translate([0,0,+6])
							roundcyl(1.5,20.0); // inside
						translate([0,9.5,0])
							roundcyl(1.0,3.0); // outside
					}
				}
		}
		interior_spaces();
	}
}

raptorthroat=0.6;
module raptorbell(outshift=0.0) {
	hull() {
		cylinder(d=raptordia+outshift,h=0.01);
		midbell=0.5*raptorheight;
		translate([0,0,raptorheight-midbell])
			cylinder(d1=raptordia*0.85+outshift,
			         d2=raptorthroat+outshift,h=raptorheight-midbell);
		
		// Rudimentary combustion chamber
		translate([0,0,raptorheight])
			cylinder(d1=raptorthroat,d2=0.1,h=raptorthroat/4);
	}
}
module raptor_centers() {
	translate([0,0,raptorstart])
	{
		children();
		for (r=[1:6]) 
			rotate([0,0,360*(r+0.5)/6])
				translate([raptorspace,0,0])
					children();
	}
}

module raptors() {
	color([0.2,0.1,0.4])
	raptor_centers() {
		difference() {
			union() {
				raptorbell(0.01);
				translate([0,0,raptorheight-0.1])
					cylinder(d=raptorthroat,h=1.0);
			}
			// Hollow out inside bell
			translate([0,0,-0.01])
				raptorbell(0.0);
			
			
		}
	}
}

translate([0,0,-15]) // put COM in tanks
{
	starship();
	// raptors();
}

