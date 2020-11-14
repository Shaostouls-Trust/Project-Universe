using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AXGeometry
{
	public class StopWatch
	{
		long startticks;
		long starttime;

		long lastticks;
		long lasttime;

		
		 

		int pcount = 0;
		int dcount = 0;

		public bool doPrint = true;

		public 	List<StopwatchMilestone> 	milestones;


		public StopWatch()
		{
			restart();
			milestones = new List<StopwatchMilestone>();
		}

		public void restart()
		{
			startticks = DateTime.Now.Ticks;
			lastticks = ticks();

			starttime =  now ();
			lasttime =   now ();
			//Debug.Log (starttime);



		}

		public void reset()
		{
			


		}

		public long ticks()
		{
			return DateTime.Now.Ticks - startticks;
		}


		/// <summary>
		/// Sets a milestone, identified by the specified label.
		/// </summary>
		/// <param name="label">Label.</param>
		public void milestone(string label)
		{
			long now = ticks();

			milestones.Add(new StopwatchMilestone(label, now, now-lastticks));

			lastticks = now;
		}

		public void dump()
		{
			
			Debug.Log("Stopwatch Dump ================");
			for (int i=0; i<milestones.Count; i++)
			{
				Debug.Log("<color=red>" + milestones[i].durationTicks.ToString().PadLeft(6)+ "</color> " + milestones[i].label );
			}
			Debug.Log("total milliseconds: " + lastticks/TimeSpan.TicksPerMillisecond);
		}

		public static long now()
		{
			return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}


		public long time()
		{
			return now()-starttime;
		}

		public void ptime()
		{
			Debug.Log ("time "+ (pcount++) + ": " +time ());
		}

		public long duration()
		{
			long duration = now () - lasttime;
			lasttime = now ();
			return duration;
		}
		public void pDuration()
		{
			if (doPrint)
			{
				Debug.Log ("duration "+ (dcount++) + ": " + duration());

			}
		}




		public long stop()
		{
			long endtime =  DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

			//Debug.Log (endtime);
			return endtime-starttime;


		}

	}




	// STOP_WATCH_MILESTONE

	public struct StopwatchMilestone
	{
		public string label;
		public long   ticks;
		public long   durationTicks;

		public StopwatchMilestone(string l, long t, long dt)
		{
			label = l;
			ticks = t;
			durationTicks = dt;
		}

	}
}

