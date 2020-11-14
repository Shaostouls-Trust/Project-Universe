using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* AX_NODE
 * Base object for the tree data structure under an AX.Model.
 * This will be best for determining the visibility of any AX.Parameter
 * because from a parameter, one can look up the tree to see if isOpen is set throughout its ancestry.
 */
namespace AX
{
	[System.Serializable]
	public class AXNode  {
	  
		[SerializeField]
		public bool isOpen 				= false;
		public bool isOpenInInspector 	= false;

		[System.NonSerialized]
		public bool hasClosedDependsOns;

		// Is this Node stowed?
		public bool isStowed = false;

		// These are used for displaying this parameter in a ParametricObject palette
		public Rect		rect;

		[System.NonSerialized]
		public Vector2	inputPoint;

		[System.NonSerialized]
		public Vector2	outputPoint;

		 
		[SerializeField]
		protected string m_name;
		public  string 	 Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		public string description;




		
		// These are set on deserialize




		[System.NonSerialized]
		public System.Type parentNodeType;

		[System.NonSerialized]
		private AXNode		_parentnode				= null;
		public 	AXNode		 ParentNode   
		{
			get  { return _parentnode; }
			set  { 
				_parentnode = value;
				if (value != null) 
					parentNodeType = value.GetType();
			}
		}

		[System.NonSerialized]
		public List<AXNode>		children				= null;

		public void addChild(AXNode newChild)
		{
			if (children == null)
				children = new List<AXNode>();
			children.Add(newChild);
			newChild.ParentNode = this;
		}







		
		// Convient accessors
		[System.NonSerialized]
		private AXParametricObject		_parametricObject = null;
		public 	AXParametricObject		 parametricObject   
		{
			get  { return _parametricObject; }
			set  { _parametricObject = value; }
		}

		[System.NonSerialized]
		private AXModel		_model = null;
		public 	AXModel		 model   
		{
			get  { return _model; }
			set  { _model = value; }
		}








		public AXNode(string n)
		{
			Name = n;
		
		}



		public bool isVisible()
		{
			if(ParentNode != null)
				return ParentNode.rootIsOpen (0);
			
			return true;
		}
		
		public bool rootIsOpen(int gov)
		{
			if (gov > 7)
				return false;
				
			
			if(isOpen && ParentNode != null)
				return ParentNode.rootIsOpen (gov++);
				
			return isOpen;
		}
		
		public void openToRoot(int gov)
		{
			
			if (ParentNode != null)
			{
				ParentNode.isOpen = true;
				ParentNode.openToRoot(gov++);
			}
		}
		public void foldToRoot(int gov)
		{
			
			if (ParentNode != null)
			{
				ParentNode.isOpen = false;
				ParentNode.openToRoot(gov++);
			}
		}






	}
}
