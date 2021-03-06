using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
	/// <summary>
	/// Standard Character Gear.
	/// </summary>
	public class Gear
	{
		protected Guid _guiID = new Guid();
		protected string _strName = "";
		protected string _strCategory = "";
		protected int _intMaxRating = 0;
		protected int _intMinRating = 0;
		protected int _intRating = 0;
		protected int _intQty = 1;
		protected string _strCapacity = "";
		protected string _strArmorCapacity = "";
		protected string _strAvail = "";
		protected string _strAvail3 = "";
		protected string _strAvail6 = "";
		protected string _strAvail10 = "";
		protected int _intCostFor = 1;
		protected int _intDeviceRating = 0;
		protected string _strCost = "";
		protected string _strCost3 = "";
		protected string _strCost6 = "";
		protected string _strCost10 = "";
		protected string _strSource = "";
		protected string _strPage = "";
		protected string _strExtra = "";
		protected bool _blnBonded = false;
		protected bool _blnEquipped = true;
		protected bool _blnHomeNode = false;
		protected XmlNode _nodBonus;
		protected XmlNode _nodWeaponBonus;
		protected Guid _guiWeaponID = new Guid();
		protected List<Gear> _objChildren = new List<Gear>();
		protected string _strNotes = "";
		protected string _strLocation = "";
		protected Character _objCharacter;
		protected string _strAltName = "";
		protected string _strAltCategory = "";
		protected string _strAltPage = "";
		private int _intChildCostMultiplier = 1;
		private int _intChildAvailModifier = 0;
		protected Gear _objParent = null;
		protected bool _blnDiscountCost = false;
		protected string _strGearName = "";
		protected bool _blnIncludedInParent = false;
		protected int _intMatrixCM = 0;
		protected int _intMatrixCMFilled = 0;

		#region Constructor, Create, Save, Load, and Print Methods
		public Gear(Character objCharacter)
		{
			// Create the GUID for the new piece of Gear.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}
		
		/// Create a Gear from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlGear">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character the Gear is being added to.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="intRating">Selected Rating for the Gear.</param>
		/// <param name="objWeapons">List of Weapons that should be added to the character.</param>
		/// <param name="objWeaponNodes">List of TreeNodes to represent the added Weapons</param>
		/// <param name="strForceValue">Value to forcefully select for any ImprovementManager prompts.</param>
		/// <param name="blnHacked">Whether or not a Matrix Program has been hacked (removing the Copy Protection and Registration plugins).</param>
		/// <param name="blnInherent">Whether or not a Program is Inherent to an A.I.</param>
		/// <param name="blnAddImprovements">Whether or not Improvements should be added to the character.</param>
		/// <param name="blnCreateChildren">Whether or not child Gear should be created.</param>
		/// <param name="blnAerodynamic">Whether or not Weapons should be created as Aerodynamic.</param>
		public void Create(XmlNode objXmlGear, Character objCharacter, TreeNode objNode, int intRating, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, string strForceValue = "", bool blnHacked = false, bool blnInherent = false, bool blnAddImprovements = true, bool blnCreateChildren = true, bool blnAerodynamic = false)
		{
			_strName = objXmlGear["name"].InnerText;
			_strCategory = objXmlGear["category"].InnerText;
			_strAvail = objXmlGear["avail"].InnerText;
			objXmlGear.TryGetField("avail3", out _strAvail3, "");
			objXmlGear.TryGetField("avail6", out _strAvail6, "");
			objXmlGear.TryGetField("avail10", out _strAvail10, "");
			objXmlGear.TryGetField("capacity", out _strCapacity, "");
			objXmlGear.TryGetField("armorcapacity", out _strArmorCapacity, "");
			objXmlGear.TryGetField("costfor", out _intCostFor, 1);
			objXmlGear.TryGetField("costfor", out _intQty, 1);
			objXmlGear.TryGetField("cost", out _strCost, "");
			objXmlGear.TryGetField("cost3", out _strCost3, "");
			objXmlGear.TryGetField("cost6", out _strCost6, "");
			objXmlGear.TryGetField("cost10", out _strCost10, "");
			_nodBonus = objXmlGear["bonus"];
			_intMaxRating = Convert.ToInt32(objXmlGear["rating"].InnerText);
			objXmlGear.TryGetField("minrating", out _intMinRating);
			_intRating = intRating;
			_strSource = objXmlGear["source"].InnerText;
			_strPage = objXmlGear["page"].InnerText;
			objXmlGear.TryGetField("childcostmultiplier", out _intChildCostMultiplier, 1);
			objXmlGear.TryGetField("childavailmodifier", out _intChildAvailModifier, 0);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("gear.xml");
				XmlNode objGearNode = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + _strName + "\"]");
				if (objGearNode != null)
				{
					if (objGearNode["translate"] != null)
						_strAltName = objGearNode["translate"].InnerText;
					if (objGearNode["altpage"] != null)
						_strAltPage = objGearNode["altpage"].InnerText;
				}

				if (_strAltName.StartsWith("Stacked Focus"))
					_strAltName = _strAltName.Replace("Stacked Focus", LanguageManager.Instance.GetString("String_StackedFocus"));

				objGearNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objGearNode != null)
				{
					if (objGearNode.Attributes["translate"] != null)
						_strAltCategory = objGearNode.Attributes["translate"].InnerText;
				}

				if (_strAltCategory.StartsWith("Stacked Focus"))
					_strAltCategory = _strAltCategory.Replace("Stacked Focus", LanguageManager.Instance.GetString("String_StackedFocus"));
			}

			// Check for a Custom name
			if (objXmlGear["name"].InnerText == "Custom Item")
			{
				frmSelectText frmPickText = new frmSelectText();
				frmPickText.PreventXPathErrors = true;
				frmPickText.Description = LanguageManager.Instance.GetString("String_CustomItem_SelectText");
				frmPickText.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickText.DialogResult != DialogResult.Cancel)
				{
					_strName = frmPickText.SelectedValue;
				}
			}
            
			// Check for a Variable Cost.
			if (objXmlGear["cost"] != null)
			{
				if (objXmlGear["cost"].InnerText.StartsWith("Variable"))
				{
					int intMin = 0;
					int intMax = 0;
					string strCost = objXmlGear["cost"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
					if (strCost.Contains("-"))
					{
						string[] strValues = strCost.Split('-');
						intMin = Convert.ToInt32(strValues[0]);
						intMax = Convert.ToInt32(strValues[1]);
					}
					else
						intMin = Convert.ToInt32(strCost.Replace("+", string.Empty));

					if (intMin != 0 || intMax != 0)
					{
						frmSelectNumber frmPickNumber = new frmSelectNumber();
						if (intMax == 0)
							intMax = 1000000;
						frmPickNumber.Minimum = intMin;
						frmPickNumber.Maximum = intMax;
						frmPickNumber.Description = LanguageManager.Instance.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
						frmPickNumber.AllowCancel = false;
						frmPickNumber.ShowDialog();
						_strCost = frmPickNumber.SelectedValue.ToString();
					}
				}
			}

			string strSource = _guiID.ToString();

			objNode.Text = _strName;
			objNode.Tag = _guiID.ToString();

			// If the Gear is Ammunition, ask the user to select a Weapon Category for it to be limited to.
			if (_strCategory == "Ammunition" && (_strName.StartsWith("Ammo:") || _strName.StartsWith("Arrow:") || _strName.StartsWith("Bolt:")))
			{
				frmSelectWeaponCategory frmPickWeaponCategory = new frmSelectWeaponCategory();
				frmPickWeaponCategory.Description = LanguageManager.Instance.GetString("String_SelectWeaponCategoryAmmo");
				if (strForceValue != "")
					frmPickWeaponCategory.OnlyCategory = strForceValue;

				//should really go in a data file
				if (_strName.StartsWith("Ammo:"))
				{
					if (_strName.StartsWith("Ammo: Assault Cannon") || _strName.StartsWith("Ammo: Gauss"))
					{
						frmPickWeaponCategory.WeaponType = "cannon";
					}
					else if (_strName.StartsWith("Ammo: Taser Dart"))
					{
						frmPickWeaponCategory.WeaponType = "taser";
					}
					else if(_strName.StartsWith("Ammo: Fuel Canister"))
					{
						frmPickWeaponCategory.WeaponType = "flame";
					}
					else if (_strName.StartsWith("Ammo: Injection Dart"))
					{
						frmPickWeaponCategory.WeaponType = "exotic";
                    }
                    else if (_strName.StartsWith("Ammo: Peak-Discharge"))
                    {
                        frmPickWeaponCategory.WeaponType = "laser";
                    }
                    else
					{
						frmPickWeaponCategory.WeaponType = "gun";
					}
				}
				else if (_strName.StartsWith("Arrow:"))
				{
					frmPickWeaponCategory.WeaponType = "bow";
				}
				else if (_strName.StartsWith("Bolt:"))
				{
					frmPickWeaponCategory.WeaponType = "crossbow";
				}
				frmPickWeaponCategory.ShowDialog();

				_strExtra = frmPickWeaponCategory.SelectedCategory;
				objNode.Text += " (" + _strExtra + ")";
			}

			// Add Gear Weapons if applicable.
			if (objXmlGear.InnerXml.Contains("<addweapon>"))
			{
				XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");

				// More than one Weapon can be added, so loop through all occurrences.
				foreach (XmlNode objXmlAddWeapon in objXmlGear.SelectNodes("addweapon"))
				{
					XmlNode objXmlWeapon = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlAddWeapon.InnerText + "\"]");

					TreeNode objGearWeaponNode = new TreeNode();
					Weapon objGearWeapon = new Weapon(objCharacter);
					objGearWeapon.Create(objXmlWeapon, objCharacter, objGearWeaponNode, null, null);
					objGearWeaponNode.ForeColor = SystemColors.GrayText;
					if (blnAerodynamic)
					{
						objGearWeapon.Name += " (" + LanguageManager.Instance.GetString("Checkbox_Aerodynamic") + ")";
						objGearWeapon.SetRange("Aerodynamic Grenades");
						objGearWeaponNode.Text = objGearWeapon.DisplayName;
						_strName += " (" + LanguageManager.Instance.GetString("Checkbox_Aerodynamic") + ")";
						objNode.Text = DisplayName;
					}
						
					objWeaponNodes.Add(objGearWeaponNode);
					objWeapons.Add(objGearWeapon);

					_guiWeaponID = Guid.Parse(objGearWeapon.InternalId);
				}
			}

			// If the item grants a bonus, pass the information to the Improvement Manager.
			if (objXmlGear.InnerXml.Contains("<bonus>"))
			{
				// Do not apply the Improvements if this is a Focus, unless we're speicifically creating a Weapon Focus. This is to avoid creating the Foci's Improvements twice (once when it's first added
				// to the character which is incorrect, and once when the Focus is actually Bonded).
				bool blnApply = !((_strCategory == "Foci" || _strCategory == "Metamagic Foci") && !objXmlGear["bonus"].InnerXml.Contains("selectweapon"));

				if (blnApply)
				{
					ImprovementManager objImprovementManager;
					if (blnAddImprovements)
						objImprovementManager = new ImprovementManager(objCharacter);
					else
						objImprovementManager = new ImprovementManager(null);

					objImprovementManager.ForcedValue = strForceValue;
					if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Gear, strSource, objXmlGear["bonus"], false, intRating, DisplayNameShort))
					{
						_guiID = Guid.Empty;
						return;
					}
					if (objImprovementManager.SelectedValue != "")
					{
						_strExtra = objImprovementManager.SelectedValue;
						objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
					}
				}
			}

			// Check to see if there are any child elements.
			if (objXmlGear.InnerXml.Contains("<gears>") && blnCreateChildren)
			{
				// Create Gear using whatever information we're given.
				foreach (XmlNode objXmlChild in objXmlGear.SelectNodes("gears/gear"))
				{
					Gear objChild = new Gear(_objCharacter);
					TreeNode objChildNode = new TreeNode();
					objChild.Name = objXmlChild["name"].InnerText;
					objChild.Category = objXmlChild["category"].InnerText;
					objChild.Avail = "0";
					objChild.Cost = "0";
					objChild.Source = _strSource;
					objChild.Page = _strPage;
					objChild.Parent = this;
					objChild.IncludedInParent = true;
					_objChildren.Add(objChild);

					objChildNode.Text = objChild.DisplayName;
					objChildNode.Tag = objChild.InternalId;
					objNode.Nodes.Add(objChildNode);
					objNode.Expand();
				}

				XmlDocument objXmlGearDocument = XmlManager.Instance.Load("gear.xml");
				CreateChildren(objXmlGearDocument, objXmlGear, this, objNode, objCharacter, blnHacked);
			}

			// Add the Copy Protection and Registration plugins to the Matrix program. This does not apply if Unwired is not enabled, Hacked is selected, or this is a Suite being added (individual programs will add it to themselves).
			if (blnCreateChildren)
			{
				if ((_strCategory == "Matrix Programs" || _strCategory == "Skillsofts" || _strCategory == "Autosofts" || _strCategory == "Autosofts, Agent" || _strCategory == "Autosofts, Drone") && objCharacter.Options.BookEnabled("UN") && !blnHacked && !_strName.StartsWith("Suite:"))
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("gear.xml");

					if (_objCharacter.Options.AutomaticCopyProtection && !blnInherent)
					{
						Gear objPlugin1 = new Gear(_objCharacter);
						TreeNode objPlugin1Node = new TreeNode();
						objPlugin1.Create(objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"Copy Protection\"]"), objCharacter, objPlugin1Node, _intRating, null, null);
						if (_intRating == 0)
							objPlugin1.Rating = 1;
						objPlugin1.Avail = "0";
						objPlugin1.Cost = "0";
						objPlugin1.Cost3 = "0";
						objPlugin1.Cost6 = "0";
						objPlugin1.Cost10 = "0";
						objPlugin1.Capacity = "[0]";
						objPlugin1.Parent = this;
						_objChildren.Add(objPlugin1);
						objNode.Nodes.Add(objPlugin1Node);
					}

					if (_objCharacter.Options.AutomaticRegistration && !blnInherent)
					{
						Gear objPlugin2 = new Gear(_objCharacter);
						TreeNode objPlugin2Node = new TreeNode();
						objPlugin2.Create(objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"Registration\"]"), objCharacter, objPlugin2Node, 0, null, null);
						objPlugin2.Avail = "0";
						objPlugin2.Cost = "0";
						objPlugin2.Cost3 = "0";
						objPlugin2.Cost6 = "0";
						objPlugin2.Cost10 = "0";
						objPlugin2.Capacity = "[0]";
						objPlugin2.Parent = this;
						_objChildren.Add(objPlugin2);
						objNode.Nodes.Add(objPlugin2Node);
						objNode.Expand();
					}

					if ((objCharacter.Metatype == "A.I." || objCharacter.MetatypeCategory == "Technocritters" || objCharacter.MetatypeCategory == "Protosapients") && blnInherent)
					{
						Gear objPlugin3 = new Gear(_objCharacter);
						TreeNode objPlugin3Node = new TreeNode();
						objPlugin3.Create(objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"Ergonomic\"]"), objCharacter, objPlugin3Node, 0, null, null);
						objPlugin3.Avail = "0";
						objPlugin3.Cost = "0";
						objPlugin3.Cost3 = "0";
						objPlugin3.Cost6 = "0";
						objPlugin3.Cost10 = "0";
						objPlugin3.Capacity = "[0]";
						objPlugin3.Parent = this;
						_objChildren.Add(objPlugin3);
						objNode.Nodes.Add(objPlugin3Node);

						Gear objPlugin4 = new Gear(_objCharacter);
						TreeNode objPlugin4Node = new TreeNode();
						objPlugin4.Create(objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"Optimization\" and category = \"Program Options\"]"), objCharacter, objPlugin4Node, _intRating, null, null);
						if (_intRating == 0)
							objPlugin4.Rating = 1;
						objPlugin4.Avail = "0";
						objPlugin4.Cost = "0";
						objPlugin4.Cost3 = "0";
						objPlugin4.Cost6 = "0";
						objPlugin4.Cost10 = "0";
						objPlugin4.Capacity = "[0]";
						objPlugin4.Parent = this;
						_objChildren.Add(objPlugin4);
						objNode.Nodes.Add(objPlugin4Node);
						objNode.Expand();
					}
				}
			}

			// If the item grants a Weapon bonus (Ammunition), just fill the WeaponBonus XmlNode.
			if (objXmlGear.InnerXml.Contains("<weaponbonus>"))
				_nodWeaponBonus = objXmlGear["weaponbonus"];
			objNode.Text = DisplayName;
		}

		protected void CreateChildren(XmlDocument objXmlGearDocument, XmlNode objXmlGear, Gear objParent, TreeNode objNode, Character objCharacter, bool blnHacked)
		{
			// Create Gear by looking up the name of the item we're provided with.
			if (objXmlGear.SelectNodes("gears/usegear").Count > 0)
			{
				foreach (XmlNode objXmlChild in objXmlGear.SelectNodes("gears/usegear"))
				{
					XmlNode objXmlGearNode = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlChild["name"].InnerText + "\" and category = \"" + objXmlChild["category"].InnerText + "\"]");
					int intChildRating = 0;
					int intChildQty = 1;
					string strChildForceValue = "";
					if (objXmlChild["rating"] != null)
						intChildRating = Convert.ToInt32(objXmlChild["rating"].InnerText);
					if (objXmlChild["name"].Attributes["qty"] != null)
						intChildQty = Convert.ToInt32(objXmlChild["name"].Attributes["qty"].InnerText);
					if (objXmlChild["name"].Attributes["select"] != null)
						strChildForceValue = objXmlChild["name"].Attributes["select"].InnerText;

					Gear objChild = new Gear(_objCharacter);
					TreeNode objChildNode = new TreeNode();
					List<Weapon> lstChildWeapons = new List<Weapon>();
					List<TreeNode> lstChildWeaponNodes = new List<TreeNode>();
					objChild.Create(objXmlGearNode, objCharacter, objChildNode, intChildRating, lstChildWeapons, lstChildWeaponNodes, strChildForceValue, blnHacked);
					objChild.Quantity = intChildQty;
					objChild.Cost = "0";
					objChild.Cost3 = "0";
					objChild.Cost6 = "0";
					objChild.Cost10 = "0";
					objChild.MinRating = intChildRating;
					objChild.MaxRating = intChildRating;
					objChild.Parent = this;
					objChild.IncludedInParent = true;
					objParent.Children.Add(objChild);

					// Change the Capacity of the child if necessary.
					if (objXmlChild["capacity"] != null)
						objChild.Capacity = "[" + objXmlChild["capacity"].InnerText + "]";

					objNode.Nodes.Add(objChildNode);
					objNode.Expand();

					CreateChildren(objXmlGearDocument, objXmlChild, objChild, objChildNode, objCharacter, blnHacked);
				}
			}
		}

		/// <summary>
		/// Copy a piece of Gear.
		/// </summary>
		/// <param name="objGear">Gear object to copy.</param>
		/// <param name="objNode">TreeNode for the copied item.</param>
		/// <param name="objWeapons">List of Weapons created by the copied item.</param>
		/// <param name="objWeaponNodes">List of TreeNodes for the Weapons created by the copied item.</param>
		public void Copy(Gear objGear, TreeNode objNode, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes)
		{
			_strName = objGear.Name;
			_strCategory = objGear.Category;
			_intMaxRating = objGear.MaxRating;
			_intMinRating = objGear.MinRating;
			_intRating = objGear.Rating;
			_intQty = objGear.Quantity;
			_strCapacity = objGear.Capacity;
			_strArmorCapacity = objGear.ArmorCapacity;
			_strAvail = objGear.Avail;
			_strAvail3 = objGear.Avail3;
			_strAvail6 = objGear.Avail6;
			_strAvail10 = objGear.Avail10;
			_intCostFor = objGear.CostFor;
			_intDeviceRating = objGear.DeviceRating;
			_strCost = objGear.Cost;
			_strCost3 = objGear.Cost3;
			_strCost6 = objGear.Cost6;
			_strCost10 = objGear.Cost10;
			_strSource = objGear.Source;
			_strPage = objGear.Page;
			_strExtra = objGear.Extra;
			_blnBonded = objGear.Bonded;
			_blnEquipped = objGear.Equipped;
			_blnHomeNode = objGear.HomeNode;
			_nodBonus = objGear.Bonus;
			_nodWeaponBonus = objGear.WeaponBonus;
			_guiWeaponID = Guid.Parse(objGear.WeaponID);
			_strNotes = objGear.Notes;
			_strLocation = objGear.Location;
			_intChildAvailModifier = objGear.ChildAvailModifier;
			_intChildCostMultiplier = objGear.ChildCostMultiplier;
			_strGearName = objGear.GearName;

			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();

			foreach (Gear objGearChild in objGear.Children)
			{
				TreeNode objChildNode = new TreeNode();
				Gear objChild = new Gear(_objCharacter);
				if (objGearChild.GetType() == typeof(Commlink))
				{
					Commlink objCommlink = new Commlink(_objCharacter);
					objCommlink.Copy(objGearChild, objChildNode, objWeapons, objWeaponNodes);
					objChild = objCommlink;
				}
				else
					objChild.Copy(objGearChild, objChildNode, objWeapons, objWeaponNodes);
				_objChildren.Add(objChild);

				objNode.Nodes.Add(objChildNode);
				objNode.Expand();
			}
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("gear");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("category", _strCategory);
			objWriter.WriteElementString("capacity", _strCapacity);
			objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
			objWriter.WriteElementString("minrating", _intMinRating.ToString());
			objWriter.WriteElementString("maxrating", _intMaxRating.ToString());
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("qty", _intQty.ToString());
			objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("avail3", _strAvail3);
			objWriter.WriteElementString("avail6", _strAvail6);
			objWriter.WriteElementString("avail10", _strAvail10);
			if (_intCostFor > 1)
				objWriter.WriteElementString("costfor", _intCostFor.ToString());
			objWriter.WriteElementString("cost", _strCost);
			objWriter.WriteElementString("cost3", _strCost3);
			objWriter.WriteElementString("cost6", _strCost6);
			objWriter.WriteElementString("cost10", _strCost10);
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("bonded", _blnBonded.ToString());
			objWriter.WriteElementString("equipped", _blnEquipped.ToString());
			objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
			if (_guiWeaponID != Guid.Empty)
				objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString());
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", "");
			if (_nodWeaponBonus != null)
				objWriter.WriteRaw("<weaponbonus>" + _nodWeaponBonus.InnerXml + "</weaponbonus>");
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("devicerating", _intDeviceRating.ToString());
			objWriter.WriteElementString("gearname", _strGearName);
			objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString());
			objWriter.WriteElementString("conditionmonitor", ConditionMonitor.ToString());
			objWriter.WriteElementString("includedinparent", _blnIncludedInParent.ToString());
			if (_intChildCostMultiplier != 1)
				objWriter.WriteElementString("childcostmultiplier", _intChildCostMultiplier.ToString());
			if (_intChildAvailModifier != 0)
				objWriter.WriteElementString("childavailmodifier", _intChildAvailModifier.ToString());
			objWriter.WriteStartElement("children");
			foreach (Gear objGear in _objChildren)
			{
				// Use the Gear's SubClass if applicable.
				if (objGear.GetType() == typeof(Commlink))
				{
					Commlink objCommlink = new Commlink(_objCharacter);
					objCommlink = (Commlink)objGear;
					objCommlink.Save(objWriter);
				}
				else
				{
					objGear.Save(objWriter);
				}
			}
			objWriter.WriteEndElement();
			objWriter.WriteElementString("location", _strLocation);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Gear from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode, bool blnCopy = false)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strCategory = objNode["category"].InnerText;
			objNode.TryGetField("matrixcmfilled", out _intMatrixCMFilled);
			objNode.TryGetField("capacity", out _strCapacity);
			objNode.TryGetField("armorcapacity", out _strArmorCapacity);
			objNode.TryGetField("minrating", out _intMinRating);
			_intMaxRating = Convert.ToInt32(objNode["maxrating"].InnerText);
			_intRating = Convert.ToInt32(objNode["rating"].InnerText);
			_intQty = Convert.ToInt32(objNode["qty"].InnerText);
			_strAvail = objNode["avail"].InnerText;
			try
			{
				_strAvail3 = objNode["avail3"].InnerText;
			}
			catch
			{
			}
			try
			{
				_strAvail6 = objNode["avail6"].InnerText;
			}
			catch
			{
			}
			try
			{
				_strAvail10 = objNode["avail10"].InnerText;
			}
			catch
			{
			}
			try
			{
				_intCostFor = Convert.ToInt32(objNode["costfor"].InnerText);
			}
			catch
			{
			}
			_strCost = objNode["cost"].InnerText;
			try
			{
				_strCost3 = objNode["cost3"].InnerText;
			}
			catch { }
			try
			{
				_strCost6 = objNode["cost6"].InnerText;
			}
			catch { }
			try
			{
				_strCost10 = objNode["cost10"].InnerText;
			}
			catch
			{
			}
			if (objNode["extra"].InnerText == "Hold-Outs")
				objNode["extra"].InnerText = "Holdouts";
			_strExtra = objNode["extra"].InnerText;
			try
			{
				_blnBonded = Convert.ToBoolean(objNode["bonded"].InnerText);
			}
			catch
			{
			}
			try
			{
				_blnEquipped = Convert.ToBoolean(objNode["equipped"].InnerText);
			}
			catch
			{
			}
			try
			{
				_blnHomeNode = Convert.ToBoolean(objNode["homenode"].InnerText);
			}
			catch
			{
			}
			_nodBonus = objNode["bonus"];
			try
			{
				_nodWeaponBonus = objNode["weaponbonus"];
			}
			catch
			{
			}
			_strSource = objNode["source"].InnerText;
			try
			{
				_strPage = objNode["page"].InnerText;
			}
			catch
			{
			}
			try
			{
				_intDeviceRating = Convert.ToInt32(objNode["devicerating"].InnerText);
			}
			catch
			{
			}
			try
			{
				_guiWeaponID = Guid.Parse(objNode["weaponguid"].InnerText);
			}
			catch
			{
			}

			try
			{
				_intChildCostMultiplier = Convert.ToInt32(objNode["childcostmultiplier"].InnerText);
			}
			catch
			{
			}
			try
			{
				_intChildAvailModifier = Convert.ToInt32(objNode["childavailmodifier"].InnerText);
			}
			catch
			{
			}

			try
			{
				_strGearName = objNode["gearname"].InnerText;
			}
			catch
			{
			}

			try
			{
				_blnIncludedInParent = Convert.ToBoolean(objNode["includedinparent"].InnerText);
			}
			catch
			{
			}

			if (objNode.InnerXml.Contains("<gear>"))
			{
				XmlNodeList nodChildren = objNode.SelectNodes("children/gear");
				foreach (XmlNode nodChild in nodChildren)
				{
					switch (nodChild["category"].InnerText)
					{
						case "Commlinks":
						case "Commlink Accessories":
						case "Cyberdecks":
						case "Rigger Command Consoles":
							Commlink objCommlink = new Commlink(_objCharacter);
							objCommlink.Load(nodChild, blnCopy);
							objCommlink.Parent = this;
							_objChildren.Add(objCommlink);
							break;
						default:
							Gear objGear = new Gear(_objCharacter);
							objGear.Load(nodChild, blnCopy);
							objGear.Parent = this;
							_objChildren.Add(objGear);
							break;
					}
				}
			}

			try
			{
				_strLocation = objNode["location"].InnerText;
			}
			catch
			{
			}

			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}

			try
			{
				_blnDiscountCost = Convert.ToBoolean(objNode["discountedcost"].InnerText);
			}
			catch
			{
			}

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("gear.xml");
				XmlNode objGearNode = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + _strName + "\"]");
				if (objGearNode != null)
				{
					if (objGearNode["translate"] != null)
						_strAltName = objGearNode["translate"].InnerText;
					if (objGearNode["altpage"] != null)
						_strAltPage = objGearNode["altpage"].InnerText;
				}

				if (_strAltName.StartsWith("Stacked Focus"))
					_strAltName = _strAltName.Replace("Stacked Focus", LanguageManager.Instance.GetString("String_StackedFocus"));

				objGearNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objGearNode != null)
				{
					if (objGearNode.Attributes["translate"] != null)
						_strAltCategory = objGearNode.Attributes["translate"].InnerText;
				}

				if (_strAltCategory.StartsWith("Stacked Focus"))
					_strAltCategory = _strAltCategory.Replace("Stacked Focus", LanguageManager.Instance.GetString("String_StackedFocus"));
			}

			if (blnCopy)
			{
				_guiID = Guid.NewGuid();
				_strLocation = string.Empty;
				_blnHomeNode = false;
			}
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("gear");
			if ((_strCategory == "Foci" || _strCategory == "Metamagic Foci") && _blnBonded)
			{
				objWriter.WriteElementString("name", DisplayNameShort + " (" + LanguageManager.Instance.GetString("Label_BondedFoci") + ")");
			}
			else
				objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("name_english", _strName);
			objWriter.WriteElementString("category", DisplayCategory);
			objWriter.WriteElementString("category_english", _strCategory);
			objWriter.WriteElementString("iscommlink", false.ToString());
			objWriter.WriteElementString("ispersona", false.ToString());
			//objWriter.WriteElementString("isnexus", (_strCategory == "Nexus").ToString());
			objWriter.WriteElementString("isammo", (_strCategory == "Ammunition").ToString());
			objWriter.WriteElementString("isprogram", IsProgram.ToString());
			objWriter.WriteElementString("isos", false.ToString());
			if (_strName == "Fake SIN")
				objWriter.WriteElementString("issin", true.ToString());
			else
				objWriter.WriteElementString("issin", false.ToString());
			objWriter.WriteElementString("capacity", _strCapacity);
			objWriter.WriteElementString("maxrating", _intMaxRating.ToString());
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("qty", _intQty.ToString());
			objWriter.WriteElementString("avail", TotalAvail(true));
			objWriter.WriteElementString("avail_english", TotalAvail(true, true));
			objWriter.WriteElementString("cost", TotalCost.ToString());
			objWriter.WriteElementString("owncost", OwnCost.ToString());
			objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
			objWriter.WriteElementString("bonded", _blnBonded.ToString());
			objWriter.WriteElementString("equipped", _blnEquipped.ToString());
			objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
			objWriter.WriteElementString("location", _strLocation);
			objWriter.WriteElementString("gearname", _strGearName);
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			objWriter.WriteStartElement("children");
			foreach (Gear objGear in _objChildren)
			{
				// Use the Gear's SubClass if applicable.
				if (objGear.GetType() == typeof(Commlink))
				{
					Commlink objCommlink = new Commlink(_objCharacter);
					objCommlink = (Commlink)objGear;
					objCommlink.Print(objWriter);
				}
				else
				{
					objGear.Print(objWriter);
				}
			}
			objWriter.WriteEndElement();
			if (_nodWeaponBonus != null)
			{
				objWriter.WriteElementString("weaponbonusdamage", WeaponBonusDamage());
				objWriter.WriteElementString("weaponbonusdamage_english", WeaponBonusDamage(true));
				objWriter.WriteElementString("weaponbonusap", WeaponBonusAP);
			}
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this piece of Gear in the Character.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Whether or not an item is an A.I.'s Home Node.
		/// </summary>
		public bool HomeNode
		{
			get
			{
				return _blnHomeNode;
			}
			set
			{
				_blnHomeNode = value;
			}
		}

		/// <summary>
		/// Guid of a Cyberware Weapon.
		/// </summary>
		public string WeaponID
		{
			get
			{
				return _guiWeaponID.ToString();
			}
			set
			{
				_guiWeaponID = Guid.Parse(value);
			}
		}

		/// <summary>
		/// Bonus node from the XML file.
		/// </summary>
		public XmlNode Bonus
		{
			get
			{
				return _nodBonus;
			}
			set
			{
				_nodBonus = value;
			}
		}

		/// <summary>
		/// WeaponBonus node from the XML file.
		/// </summary>
		public XmlNode WeaponBonus
		{
			get
			{
				return _nodWeaponBonus;
			}
			set
			{
				_nodWeaponBonus = value;
			}
		}

		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// A custom name for the Gear assigned by the player.
		/// </summary>
		public string GearName
		{
			get
			{
				return _strGearName;
			}
			set
			{
				_strGearName = value;
			}
		}

		/// <summary>
		/// Translated Category.
		/// </summary>
		public string DisplayCategory
		{
			get
			{
				string strReturn = _strCategory;
				if (_strAltCategory != string.Empty)
					strReturn = _strAltCategory;

				return strReturn;
			}
		}

		/// <summary>
		/// Category.
		/// </summary>
		public string Category
		{
			get
			{
				return _strCategory;
			}
			set
			{
				_strCategory = value;
			}
		}

		/// <summary>
		/// Gear capacity.
		/// </summary>
		public string Capacity
		{
			get
			{
				return _strCapacity;
			}
			set
			{
				_strCapacity = value;
			}
		}

		/// <summary>
		/// Armor capacity.
		/// </summary>
		public string ArmorCapacity
		{
			get
			{
				return _strArmorCapacity;
			}
			set
			{
				_strArmorCapacity = value;
			}
		}

		/// <summary>
		/// Minimum Rating.
		/// </summary>
		public int MinRating
		{
			get
			{
				return _intMinRating;
			}
			set
			{
				_intMinRating = value;
			}
		}

		/// <summary>
		/// Maximum Rating.
		/// </summary>
		public int MaxRating
		{
			get
			{
				return _intMaxRating;
			}
			set
			{
				_intMaxRating = value;
			}
		}

		/// <summary>
		/// Rating.
		/// </summary>
		public int Rating
		{
			get
			{
				return _intRating;
			}
			set
			{
				_intRating = value;
				// Make sure we don't go over the maximum Rating.
				if (_intRating > _intMaxRating)
					_intRating = _intMaxRating;
			}
		}

		/// <summary>
		/// Quantity.
		/// </summary>
		public int Quantity
		{
			get
			{
				return _intQty;
			}
			set
			{
				_intQty = value;
			}
		}

		/// <summary>
		/// Availability.
		/// </summary>
		public string Avail
		{
			get
			{
				return _strAvail;
			}
			set
			{
				_strAvail = value;
			}
		}

		/// <summary>
		/// Availability for up to Rating 3.
		/// </summary>
		public string Avail3
		{
			get
			{
				return _strAvail3;
			}
			set
			{
				_strAvail3 = value;
			}
		}

		/// <summary>
		/// Availability for up to Rating 6.
		/// </summary>
		public string Avail6
		{
			get
			{
				return _strAvail6;
			}
			set
			{
				_strAvail6 = value;
			}
		}

		/// <summary>
		/// Availability for up to Rating 10.
		/// </summary>
		public string Avail10
		{
			get
			{
				return _strAvail10;
			}
			set
			{
				_strAvail10 = value;
			}
		}

		/// <summary>
		/// Use for ammo. The number of rounds that the nuyen amount buys.
		/// </summary>
		public int CostFor
		{
			get
			{
				return _intCostFor;
			}
			set
			{
				_intCostFor = value;
			}
		}

		/// <summary>
		/// Cost.
		/// </summary>
		public string Cost
		{
			get
			{
				if (_strCost.StartsWith("FixedValues"))
				{
					string[] strValues = _strCost.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					string strCost = "0";
					if (_intRating > 0)
						strCost = strValues[Convert.ToInt32(_intRating) - 1].Replace("[", string.Empty).Replace("]", string.Empty);
					return strCost;
				}
				else if (_strCost.StartsWith("Parent Cost"))
				{

					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCostExpression = _strCost;
					string strCost = "0";

					if (_objParent == null)
					{
						return strCost;
					}
					else
					{
						strCost = strCostExpression.Replace("Weapon Cost", _objParent.Cost.ToString());
					}
					return strCost;
				}
				else
					return _strCost;
			}
			set
			{
				_strCost = value;
			}
		}

		/// <summary>
		/// Cost for up to Rating 3.
		/// </summary>
		public string Cost3
		{
			get
			{
				return _strCost3;
			}
			set
			{
				_strCost3 = value;
			}
		}

		/// <summary>
		/// Cost for up to Rating 6.
		/// </summary>
		public string Cost6
		{
			get
			{
				return _strCost6;
			}
			set
			{
				_strCost6 = value;
			}
		}

		/// <summary>
		/// Cost for up to Rating 10.
		/// </summary>
		public string Cost10
		{
			get
			{
				return _strCost10;
			}
			set
			{
				_strCost10 = value;
			}
		}

		/// <summary>
		/// Value that was selected during an ImprovementManager dialogue.
		/// </summary>
		public string Extra
		{
			get
			{
				return _strExtra;
			}
			set
			{
				_strExtra = value;
			}
		}

		/// <summary>
		/// Whether or not the Foci is bonded.
		/// </summary>
		public bool Bonded
		{
			get
			{
				return _blnBonded;
			}
			set
			{
				_blnBonded = value;
			}
		}

		/// <summary>
		/// Whether or not the Gear is equipped.
		/// </summary>
		public bool Equipped
		{
			get
			{
				return _blnEquipped;
			}
			set
			{
				_blnEquipped = value;
			}
		}

		/// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Sourcebook Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;
				if (_strAltPage != string.Empty)
					strReturn = _strAltPage;

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// A List of child pieces of Gear.
		/// </summary>
		public List<Gear> Children
		{
			get
			{
				return _objChildren;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// Device Rating.
		/// </summary>
		public int DeviceRating
		{
			get
			{
				int intDeviceRating = _intDeviceRating;
				foreach (Commlink objCommlink in _objChildren.OfType<Commlink>())
				{
					if (objCommlink.Category == "Commlink Upgrade")
					{
						if (objCommlink.DeviceRating > intDeviceRating)
							intDeviceRating = objCommlink.DeviceRating;
					}
				}

				return intDeviceRating;
			}
			set
			{
				_intDeviceRating = value;
			}
		}

		/// <summary>
		/// Location.
		/// </summary>
		public string Location
		{
			get
			{
				return _strLocation;
			}
			set
			{
				_strLocation = value;
			}
		}

		/// <summary>
		/// Whether or not the Gear qualifies as a Program in the printout XML.
		/// </summary>
		public bool IsProgram
		{
			get
			{
				if (_strCategory == "ARE Programs" || _strCategory.StartsWith("Autosofts") || _strCategory == "Data Software" || _strCategory == "Malware" || _strCategory == "Matrix Programs" || _strCategory == "Tactical AR Software" || _strCategory == "Telematics Infrastructure Software" || _strCategory == "Sensor Software")
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Whether or not the Gear has the Ergonomic Program Option.
		/// </summary>
		public bool IsErgonomic
		{
			get
			{
				foreach (Gear objPlugin in _objChildren)
				{
					if (objPlugin.Name == "Ergonomic")
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Cost multiplier for Children attached to this Gear.
		/// </summary>
		public int ChildCostMultiplier
		{
			get
			{
				return _intChildCostMultiplier;
			}
			set
			{
				_intChildCostMultiplier = value;
			}
		}

		/// <summary>
		/// Avail modifier for Children attached to this Gear.
		/// </summary>
		public int ChildAvailModifier
		{
			get
			{
				return _intChildAvailModifier;
			}
			set
			{
				_intChildAvailModifier = value;
			}
		}

		/// <summary>
		/// Parent Gear.
		/// </summary>
		public Gear Parent
		{
			get
			{
				return _objParent;
			}
			set
			{
				_objParent = value;
			}
		}

		/// <summary>
		/// Whether or not the Gear's cost should be discounted by 10% through the Black Market Pipeline Quality.
		/// </summary>
		public bool DiscountCost
		{
			get
			{
				return _blnDiscountCost;
			}
			set
			{
				_blnDiscountCost = value;
			}
		}

		/// <summary>
		/// Whether or not the Gear is included in its parent item when purchased (currently applies to Armor only).
		/// </summary>
		public bool IncludedInParent
		{
			get
			{
				return _blnIncludedInParent;
			}
			set
			{
				_blnIncludedInParent = value;
			}
		}
		#endregion

		#region Complex Properties
		/// <summary>
		/// Total Availablility of the Gear and its accessories.
		/// </summary>
		public string TotalAvail(bool blnCalculateAdditions = false, bool blnForceEnglish = false)
		{
			if (_strAvail == "")
				_strAvail = "0";

			bool blnIncludePlus = false;

			// If the Avail contains "+", return the base string and don't try to calculate anything since we're looking at a child component.
			if (_strAvail.StartsWith("+"))
			{
				blnIncludePlus = true;
				if (!blnCalculateAdditions)
					return _strAvail;
			}

			string strCalculated = "";
			string strReturn = "";

			if (_strAvail.Contains("Rating") || _strAvail3.Contains("Rating") || _strAvail6.Contains("Rating") || _strAvail10.Contains("Rating"))
			{
				// If the availability is determined by the Rating, evaluate the expression.
				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();

				string strAvail = "";
				string strAvailExpr = _strAvail;
				if (blnIncludePlus)
					strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);

				if (_intRating <= 3 && _strAvail3 != "")
					strAvailExpr = _strAvail3;
				else if (_intRating <= 6 && _strAvail6 != "")
					strAvailExpr = _strAvail6;
				else if (_intRating >= 7 && _strAvail10 != "")
					strAvailExpr = _strAvail10;

				if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
				{
					strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
					// Remove the trailing character if it is "F" or "R".
					strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
				}
				XPathExpression xprAvail = nav.Compile(strAvailExpr.Replace("Rating", _intRating.ToString()));
				strCalculated = Convert.ToInt32(nav.Evaluate(xprAvail)).ToString() + strAvail;
			}
			else
			{
				// Just a straight cost, so return the value.
				string strAvail = "";
				if (_strAvail.Contains("F") || _strAvail.Contains("R"))
				{
					strAvail = _strAvail.Substring(_strAvail.Length - 1, 1);
					strCalculated = Convert.ToInt32(_strAvail.Substring(0, _strAvail.Length - 1)) + strAvail;
				}
				else
					strCalculated = Convert.ToInt32(_strAvail).ToString();
			}

			int intAvail = 0;
			string strAvailText = "";
			if (strCalculated.Contains("F") || strCalculated.Contains("R"))
			{
				strAvailText = strCalculated.Substring(strCalculated.Length - 1);
				intAvail = Convert.ToInt32(strCalculated.Replace(strAvailText, string.Empty));
			}
			else
				intAvail = Convert.ToInt32(strCalculated);

			// Run through the child items and increase the Avail by any Mod whose Avail contains "+".
			foreach (Gear objChild in _objChildren)
			{
				if (objChild.Avail.StartsWith("+"))
				{
					string strAvail = objChild.Avail.Replace("Rating", objChild.Rating.ToString());
					strAvail = strAvail.Substring(1).Trim();
					if (strAvail.Contains("R") || strAvail.Contains("F"))
					{
						if (strAvailText != "F")
							strAvailText = strAvail.Substring(strAvail.Length - 1);
						XmlDocument objXmlDocument = new XmlDocument();
						XPathNavigator nav = objXmlDocument.CreateNavigator();
						XPathExpression xprAvail = nav.Compile(strAvail.Replace("F", string.Empty).Replace("R", string.Empty));
						intAvail += Convert.ToInt32(nav.Evaluate(xprAvail));
					}
					else
					{
						XmlDocument objXmlDocument = new XmlDocument();
						XPathNavigator nav = objXmlDocument.CreateNavigator();
						XPathExpression xprAvail = nav.Compile(strAvail.Replace("F", string.Empty).Replace("R", string.Empty));
						intAvail += Convert.ToInt32(nav.Evaluate(xprAvail));
					}
				}
			}

			// Add any Avail modifier that comes from its Parent.
			if (_objParent != null)
				intAvail += _objParent.ChildAvailModifier;

			strReturn = intAvail.ToString() + strAvailText;

			// Translate the Avail string.
			if (!blnForceEnglish)
			{
				strReturn = strReturn.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted"));
				strReturn = strReturn.Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));
			}

			if (blnIncludePlus)
				strReturn = "+" + strReturn;

			return strReturn;
		}

		/// <summary>
		/// Caculated Capacity of the Gear.
		/// </summary>
		public string CalculatedCapacity
		{
			get
			{
				if (_strCapacity.Contains("/["))
				{
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					int intPos = _strCapacity.IndexOf("/[");
					string strFirstHalf = _strCapacity.Substring(0, intPos);
					string strSecondHalf = _strCapacity.Substring(intPos + 1, _strCapacity.Length - intPos - 1);
					bool blnSquareBrackets = false;
					string strCapacity = "";

					try
					{
						blnSquareBrackets = strFirstHalf.Contains('[');
						strCapacity = strFirstHalf;
						if (blnSquareBrackets)
							strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
					}
					catch
					{
					}
					XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

					string strReturn = "";
					try
					{
						if (_strCapacity == "[*]")
							strReturn = "*";
						else
						{
							if (_strCapacity.StartsWith("FixedValues"))
							{
								string[] strValues = _strCapacity.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
								strReturn = strValues[Convert.ToInt32(_intRating) - 1];
							}
							else
								strReturn = nav.Evaluate(xprCapacity).ToString();
						}
						if (blnSquareBrackets)
							strReturn = "[" + strCapacity + "]";
					}
					catch
					{
						strReturn = "0";
					}
					strReturn += "/" + strSecondHalf;
					return strReturn;
				}
				else if (_strCapacity.Contains("Rating"))
				{
					// If the Capaicty is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					// XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
					bool blnSquareBrackets = _strCapacity.Contains('[');
					string strCapacity = _strCapacity;
					if (blnSquareBrackets)
						strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
					XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

					string strReturn = "";

					// This has resulted in a non-whole number, so round it (minimum of 1).
					decimal decNumber = Convert.ToDecimal(nav.Evaluate(xprCapacity), GlobalOptions.Instance.CultureInfo);
					int intNumber = Convert.ToInt32(Math.Ceiling(decNumber));
					if (intNumber < 1)
						intNumber = 1;
					strReturn = intNumber.ToString();

					if (blnSquareBrackets)
						strReturn = "[" + strReturn + "]";

					return strReturn;
				}
				else
				{
					// Just a straight Capacity, so return the value.
					if (_strCapacity == "")
						return "0";
					else
						return _strCapacity;
				}
			}
		}

		/// <summary>
		/// Caculated Capacity of the Gear when attached to Armor.
		/// </summary>
		public string CalculatedArmorCapacity
		{
			get
			{
				if (_strArmorCapacity.Contains("/["))
				{
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					int intPos = _strArmorCapacity.IndexOf("/[");
					string strFirstHalf = _strArmorCapacity.Substring(0, intPos);
					string strSecondHalf = _strArmorCapacity.Substring(intPos + 1, _strArmorCapacity.Length - intPos - 1);
					bool blnSquareBrackets = false;
					string strCapacity = "";

					try
					{
						blnSquareBrackets = strFirstHalf.Contains('[');
						strCapacity = strFirstHalf;
						if (blnSquareBrackets)
							strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
					}
					catch
					{
					}
					XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

					string strReturn = "";
					try
					{
						if (_strArmorCapacity == "[*]")
							strReturn = "*";
						else
						{
							if (_strArmorCapacity.StartsWith("FixedValues"))
							{
								string[] strValues = _strArmorCapacity.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
								strReturn = strValues[Convert.ToInt32(_intRating) - 1];
							}
							else
								strReturn = nav.Evaluate(xprCapacity).ToString();
						}
						if (blnSquareBrackets)
							strReturn = "[" + strCapacity + "]";
					}
					catch
					{
						strReturn = "0";
					}
					strReturn += "/" + strSecondHalf;
					return strReturn;
				}
				else if (_strArmorCapacity.Contains("Rating"))
				{
					// If the Capaicty is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					// XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
					bool blnSquareBrackets = _strArmorCapacity.Contains('[');
					string strCapacity = _strArmorCapacity;
					if (blnSquareBrackets)
						strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
					XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

					string strReturn = nav.Evaluate(xprCapacity).ToString();
					if (blnSquareBrackets)
						strReturn = "[" + strReturn + "]";

					return strReturn;
				}
				else
				{
					// Just a straight Capacity, so return the value.
					if (_strArmorCapacity == "")
						return "0";
					else
						return _strArmorCapacity;
				}
			}
		}

		/// <summary>
		/// Total cost of the just the Gear itself.
		/// </summary>
		public int CalculatedCost
		{
			get
			{
				int intReturn = 0;

				if (_strCost.Contains("Gear Cost") || _strCost3.Contains("Gear Cost") || _strCost6.Contains("Gear Cost") || _strCost10.Contains("Gear Cost"))
				{
					if (_objParent != null)
					{
						string strCostExpression = "";

						if (_strCost != "")
							strCostExpression = _strCost;
						else
						{
							if (_intRating <= 3)
								strCostExpression = _strCost3;
							else if (_intRating <= 6)
								strCostExpression = _strCost6;
							else
								strCostExpression = _strCost10;
						}

						XmlDocument objXmlDocument = new XmlDocument();
						XPathNavigator nav = objXmlDocument.CreateNavigator();
						string strCost = "";
						strCost = strCostExpression.Replace("Gear Cost", _objParent.CalculatedCost.ToString());
						XPathExpression xprCost = nav.Compile(strCost);
						intReturn = Convert.ToInt32(nav.Evaluate(xprCost).ToString());
					}
					else
						intReturn = 0;
				}
				else if (_strCost.Contains("Rating") || _strCost3.Contains("Rating") || _strCost6.Contains("Rating") || _strCost10.Contains("Rating"))
				{
					// If the cost is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCost = "";
					string strCostExpression = "";

					if (_strCost != "")
						strCostExpression = _strCost;
					else
					{
						if (_intRating <= 3)
							strCostExpression = _strCost3;
						else if (_intRating <= 6)
							strCostExpression = _strCost6;
						else
							strCostExpression = _strCost10;
					}

					strCost = strCostExpression.Replace("Rating", _intRating.ToString());
					XPathExpression xprCost = nav.Compile(strCost);
					// This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
					double dblCost = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));
					intReturn = Convert.ToInt32(dblCost);
				}
				else
				{
					// Just a straight cost, so return the value.
					intReturn = Convert.ToInt32(_strCost);
				}

				// The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
				return (intReturn * _intQty) / _intCostFor;
			}
		}

		/// <summary>
		/// Total cost of the Gear and its accessories.
		/// </summary>
		public int TotalCost
		{
			get
			{
				int intReturn = 0;
				int intGearCost = 0;

				if (_strCost.Contains("Gear Cost") || _strCost3.Contains("Gear Cost") || _strCost6.Contains("Gear Cost") || _strCost10.Contains("Gear Cost"))
				{
					if (_objParent != null)
					{
						string strCostExpression = "";

						if (_strCost != "")
							strCostExpression = _strCost;
						else
						{
							if (_intRating <= 3)
								strCostExpression = _strCost3;
							else if (_intRating <= 6)
								strCostExpression = _strCost6;
							else
								strCostExpression = _strCost10;
						}

						XmlDocument objXmlDocument = new XmlDocument();
						XPathNavigator nav = objXmlDocument.CreateNavigator();
						string strCost = "";
						strCost = strCostExpression.Replace("Gear Cost", _objParent.CalculatedCost.ToString());
						XPathExpression xprCost = nav.Compile(strCost);
						intReturn = Convert.ToInt32(nav.Evaluate(xprCost).ToString());
					}
					else
						intReturn = 0;
				}
				else if (_strCost.StartsWith("Parent Cost"))
				{

					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCostExpression = _strCost;
					string strCost = "0";

					strCost = strCostExpression.Replace("Parent Cost", _objParent.Cost.ToString());
					XPathExpression xprCost = nav.Compile(strCost);
					// This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
					double dblCost = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));
					intReturn = Convert.ToInt32(dblCost);
				}
				else if (_strCost.Contains("Rating") || _strCost3.Contains("Rating") || _strCost6.Contains("Rating") || _strCost10.Contains("Rating") || _strCost.Contains("*") || _strCost3.Contains("*") || _strCost6.Contains("*") || _strCost10.Contains("*"))
				{
					// If the cost is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCost = "";
					string strCostExpression = "";

					if (_strCost != "")
						strCostExpression = _strCost;
					else
					{
						if (_intRating <= 3)
							strCostExpression = _strCost3;
						else if (_intRating <= 6)
							strCostExpression = _strCost6;
						else
							strCostExpression = _strCost10;
					}

					strCost = strCostExpression.Replace("Rating", _intRating.ToString());
					XPathExpression xprCost = nav.Compile(strCost);
					// This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
					double dblCost = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));
					intReturn = Convert.ToInt32(dblCost);
				}
				else
				{
					// Just a straight cost, so return the value.
					if (_strCost == string.Empty)
						intReturn = 0;
					else if (_strCost.StartsWith("FixedValues"))
					{
						string[] strValues = _strCost.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
						string strCost = "0";
						if (_intRating > 0)
							strCost = strValues[Convert.ToInt32(_intRating) - 1].Replace("[", string.Empty).Replace("]", string.Empty);
						intReturn = Convert.ToInt32(strCost);
					}
					else
						intReturn = Convert.ToInt32(_strCost);
				}
				intGearCost = intReturn;

				if (DiscountCost)
					intReturn = Convert.ToInt32(Convert.ToDouble(intReturn, GlobalOptions.Instance.CultureInfo) * 0.9);

				// Add in the cost of all child components.
				int intPlugin = 0;
				foreach (Gear objChild in _objChildren)
					intPlugin += objChild.TotalCost;

				// The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
				int intParentMultiplier = 1;
				if (_objParent != null)
					intParentMultiplier = _objParent.ChildCostMultiplier;

				intReturn = (intReturn * _intQty * intParentMultiplier) / _intCostFor;
				// Add in the cost of the plugins separate since their value is not based on the Cost For number (it is always cost x qty).
				intReturn += intPlugin * _intQty;

				return intReturn;
			}
		}

		/// <summary>
		/// The cost of just the Gear itself.
		/// </summary>
		public int OwnCost
		{
			get
			{
				int intReturn = 0;
				int intGearCost = 0;

				if (_strCost.Contains("Gear Cost") || _strCost3.Contains("Gear Cost") || _strCost6.Contains("Gear Cost") || _strCost10.Contains("Gear Cost"))
				{
					if (_objParent != null)
					{
						string strCostExpression = "";

						if (_strCost != "")
							strCostExpression = _strCost;
						else
						{
							if (_intRating <= 3)
								strCostExpression = _strCost3;
							else if (_intRating <= 6)
								strCostExpression = _strCost6;
							else
								strCostExpression = _strCost10;
						}

						XmlDocument objXmlDocument = new XmlDocument();
						XPathNavigator nav = objXmlDocument.CreateNavigator();
						string strCost = "";
						strCost = strCostExpression.Replace("Gear Cost", _objParent.CalculatedCost.ToString());
						XPathExpression xprCost = nav.Compile(strCost);
						intReturn = Convert.ToInt32(nav.Evaluate(xprCost).ToString());
					}
					else
						intReturn = 0;
				}
				else if (_strCost.Contains("Rating") || _strCost3.Contains("Rating") || _strCost6.Contains("Rating") || _strCost10.Contains("Rating") || _strCost.Contains("*") || _strCost3.Contains("*") || _strCost6.Contains("*") || _strCost10.Contains("*"))
				{
					// If the cost is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCost = "";
					string strCostExpression = "";

					if (_strCost != "")
						strCostExpression = _strCost;
					else
					{
						if (_intRating <= 3)
							strCostExpression = _strCost3;
						else if (_intRating <= 6)
							strCostExpression = _strCost6;
						else
							strCostExpression = _strCost10;
					}

					strCost = strCostExpression.Replace("Rating", _intRating.ToString());
					XPathExpression xprCost = nav.Compile(strCost);
					// This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
					double dblCost = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));
					intReturn = Convert.ToInt32(dblCost);
				}
				else
				{
					// Just a straight cost, so return the value.
					if (_strCost == string.Empty)
						intReturn = 0;
					else
					{
						if (_strCost.StartsWith("FixedValues"))
						{
							string[] strValues = _strCost.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
							string strCost = "0";
							if (_intRating > 0)
								strCost = strValues[Convert.ToInt32(_intRating) - 1].Replace("[", string.Empty).Replace("]", string.Empty);
							intReturn = Convert.ToInt32(strCost);
						}
						else
							intReturn = Convert.ToInt32(_strCost);
					}
				}
				intGearCost = intReturn;

				if (DiscountCost)
					intReturn = Convert.ToInt32(Convert.ToDouble(intReturn, GlobalOptions.Instance.CultureInfo) * 0.9);

				// The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
				int intParentMultiplier = 1;
				if (_objParent != null)
					intParentMultiplier = _objParent.ChildCostMultiplier;

				intReturn = (intReturn * intParentMultiplier) / _intCostFor;

				return intReturn;
			}
		}

		/// <summary>
		/// The Gear's Capacity cost if used as a plugin.
		/// </summary>
		public int PluginCapacity
		{
			get
			{
				string strCapacity = CalculatedCapacity;
				if (strCapacity.Contains("/["))
				{
					// If this is a multiple-capacity item, use only the second half.
					int intPos = strCapacity.IndexOf("/[");
					strCapacity = strCapacity.Substring(intPos + 1);
				}

				// Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
				if (strCapacity.Contains("["))
					strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
				else
					strCapacity = "0";
				return Convert.ToInt32(strCapacity);
			}
		}

		/// <summary>
		/// The Gear's Capacity cost if used as an Armor plugin.
		/// </summary>
		public int PluginArmorCapacity
		{
			get
			{
				string strCapacity = CalculatedArmorCapacity;
				if (strCapacity.Contains("/["))
				{
					// If this is a multiple-capacity item, use only the second half.
					int intPos = strCapacity.IndexOf("/[");
					strCapacity = strCapacity.Substring(intPos + 1);
				}

				// Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
				if (strCapacity.Contains("["))
					strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
				else
					strCapacity = "0";
				return Convert.ToInt32(strCapacity);
			}
		}

		/// <summary>
		/// The amount of Capacity remaining in the Gear.
		/// </summary>
		public int CapacityRemaining
		{
			get
			{
				int intCapacity = 0;
				if (!_strCapacity.Contains("[") || _strCapacity.Contains("/["))
				{
					// Get the Gear base Capacity.
					if (_strCapacity.Contains("/["))
					{
						// If this is a multiple-capacity item, use only the first half.
						string strMyCapacity = CalculatedCapacity;
						int intPos = strMyCapacity.IndexOf("/[");
						strMyCapacity = strMyCapacity.Substring(0, intPos);
						intCapacity = Convert.ToInt32(strMyCapacity);
					}
					else
						intCapacity = Convert.ToInt32(CalculatedCapacity);

					// Run through its Children and deduct the Capacity costs.
					foreach (Gear objChildGear in Children)
					{
						string strCapacity = objChildGear.CalculatedCapacity;
						if (strCapacity.Contains("/["))
						{
							// If this is a multiple-capacity item, use only the second half.
							int intPos = strCapacity.IndexOf("/[");
							strCapacity = strCapacity.Substring(intPos + 1);
						}

						// Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
						if (strCapacity.Contains("["))
							strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
						else
							strCapacity = "0";
						intCapacity -= (Convert.ToInt32(strCapacity) * objChildGear.Quantity);
					}
				}

				return intCapacity;
			}
		}

		/// <summary>
		/// The name of the object as it should appear on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				if (_strAltName != string.Empty)
					strReturn = _strAltName;

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				if (_intQty > 1)
					strReturn = _intQty.ToString() + " " + strReturn;
				if (_intRating > 0)
					strReturn += " (" + LanguageManager.Instance.GetString("String_Rating") + " " + _intRating.ToString() + ")";
				if (_strExtra != "")
					strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";

				if (_strGearName != "")
				{
					strReturn += " (\"" + _strGearName + "\")";
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Weapon Bonus Damage.
		/// </summary>
		public string WeaponBonusDamage(bool blnForceEnglish = false)
		{
			if (_nodWeaponBonus == null)
				return "";
			else
			{
				string strReturn = "";
				// Use the damagereplace value if applicable.
				if (_nodWeaponBonus["damagereplace"] != null)
					strReturn = _nodWeaponBonus["damagereplace"].InnerText;
				else
				{
					// Use the damage bonus if available, otherwise use 0.
					if (_nodWeaponBonus["damage"] != null)
						strReturn = _nodWeaponBonus["damage"].InnerText;
					else
						strReturn = "0";

					// Attach the type if applicable.
					if (_nodWeaponBonus["damagetype"] != null)
						strReturn += _nodWeaponBonus["damagetype"].InnerText;

					// If this does not start with "-", add a "+" to the string.
					if (!strReturn.StartsWith("-"))
						strReturn = "+" + strReturn;
				}

				// Translate the Avail string.
				if (!blnForceEnglish)
				{
					strReturn = strReturn.Replace("P", LanguageManager.Instance.GetString("String_DamagePhysical"));
					strReturn = strReturn.Replace("S", LanguageManager.Instance.GetString("String_DamageStun"));
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Weapon Bonus AP.
		/// </summary>
		public string WeaponBonusAP
		{
			get
			{
				if (_nodWeaponBonus == null)
					return "";
				else
				{
					string strReturn = "";
					// Use the apreplace value if applicable.
					if (_nodWeaponBonus["apreplace"] != null)
						strReturn = _nodWeaponBonus["apreplace"].InnerText;
					else
					{
						// Use the ap bonus if available, otherwise use 0.
						if (_nodWeaponBonus["ap"] != null)
							strReturn = _nodWeaponBonus["ap"].InnerText;
						else
							strReturn = "0";

						// If this does not start with "-", add a "+" to the string.
						if (!strReturn.StartsWith("-"))
							strReturn = "+" + strReturn;
					}

					return strReturn;
				}
			}
		}

		/// <summary>
		/// Weapon Bonus Range.
		/// </summary>
		public int WeaponBonusRange
		{
			get
			{
				if (_nodWeaponBonus == null)
					return 0;
				else
				{
					int intReturn = 0;

					if (_nodWeaponBonus["rangebonus"] != null)
						intReturn = Convert.ToInt32(_nodWeaponBonus["rangebonus"].InnerText);

					return intReturn;
				}
			}
		}


		/// <summary>
		/// Base Matrix Boxes.
		/// </summary>
		public int BaseMatrixBoxes
		{
			get
			{
				int baseMatrixBoxes = 8;
				return baseMatrixBoxes;
			}
		}

		/// <summary>
		/// Physical Condition Monitor boxes.
		/// </summary>
		public int MatrixCM
		{
			get
			{
				return BaseMatrixBoxes + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_intDeviceRating, GlobalOptions.Instance.CultureInfo) / 2.0));
			}
		}

		/// <summary>
		/// Matrix Condition Monitor boxes filled.
		/// </summary>
		public int MatrixCMFilled
		{
			get
			{
				return _intMatrixCMFilled;
			}
			set
			{
				_intMatrixCMFilled = value;
			}
		}

		/// <summary>
		/// Matrix Condition Monitor for the Commlink.
		/// </summary>
		public int ConditionMonitor
		{
			get
			{
				double dblSystem = Math.Ceiling(Convert.ToDouble(DeviceRating, GlobalOptions.Instance.CultureInfo) / 2);
				int intSystem = Convert.ToInt32(dblSystem, GlobalOptions.Instance.CultureInfo);
				return 8 + intSystem;
			}
		}
		#endregion

		#region Methods

		public IEnumerable<Gear> ThisAndAllChildren()
		{
			yield return this;

			foreach (Gear objChild in _objChildren)
			{
				foreach (Gear child in objChild.ThisAndAllChildren())
				{
					yield return child;
				}
			}
		}

		#endregion
	}
}