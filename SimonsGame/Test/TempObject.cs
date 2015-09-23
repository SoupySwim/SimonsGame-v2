using SimonsGame.Menu.InGame;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Test
{
	public static class TempObject
	{
		public static List<LevelUpNode> GetLevelUpNodes()
		{
			List<LevelUpNode> nodes = new List<LevelUpNode>();
			List<UnlockMagicNode> magicUpgrades = new List<UnlockMagicNode>();
			UnlockMagicNode upgrades = new UnlockMagicNode("Upgrades", null);
			upgrades.AlreadyPurchased = true;
			nodes.Add(upgrades);

			UnlockMagicNode base1 = new UnlockMagicNode("Choice1", null);
			base1.AlreadyPurchased = true;
			upgrades.ChildNodes.Add(base1);
			base1.ParentNode = upgrades;
			UnlockMagicNode base2 = new UnlockMagicNode("Choice2", null);
			base2.AlreadyPurchased = true;
			base2.ParentNode = upgrades;
			upgrades.ChildNodes.Add(base2);

			nodes.Add(GetSelfUpgrades());

			LevelUpNode lightningNode = GetBaseAbilityForElement(Element.Lightning, "LightningBall");
			lightningNode.ChildNodes.Add(GetStunLevelUp(lightningNode.MagicName, lightningNode));
			UnlockMagicNode lightningUnlock = new UnlockMagicNode("LightningBall", lightningNode);
			base1.ChildNodes.Add(lightningUnlock);
			lightningUnlock.ParentNode = base1;
			//nodes.Add(lightningNode);

			LevelUpNode fireNode = GetBaseAbilityForElement(Element.Fire, "FireBall");
			fireNode.ChildNodes.Add(GetSlowLevelUp(fireNode.MagicName, fireNode));
			UnlockMagicNode fireUnlock = new UnlockMagicNode("FireBall", fireNode);
			base1.ChildNodes.Add(fireUnlock);
			fireUnlock.ParentNode = base1;
			//nodes.Add(fireNode);

			LevelUpNode rockNode = GetBaseAbilityForElement(Element.Rock, "RockBall");
			UnlockMagicNode rockUnlock = new UnlockMagicNode("RockBall", rockNode);
			base1.ChildNodes.Add(rockUnlock);
			rockUnlock.ParentNode = base1;
			//nodes.Add(rockNode);

			LevelUpNode ballNode = GetBaseAbilityForBall(Element.Fire, "Ball");
			UnlockMagicNode ballUnlock = new UnlockMagicNode("Ball", ballNode);
			base1.ChildNodes.Add(ballUnlock);
			ballUnlock.ParentNode = base1;
			//nodes.Add(ballNode);


			LevelUpNode pull = GetMagicBase("Pull");
			UnlockMagicNode pullUnlock = new UnlockMagicNode("Pull", pull);
			base2.ChildNodes.Add(pullUnlock);
			pullUnlock.ParentNode = base2;
			//nodes.Add(pull);

			LevelUpNode push = GetMagicBase("Push");
			UnlockMagicNode pushUnlock = new UnlockMagicNode("Push", push);
			pullUnlock.ChildNodes.Add(pushUnlock);
			pushUnlock.ParentNode = pullUnlock;
			//nodes.Add(push);

			LevelUpNode speedUp = GetMagicBase("SpeedUp");
			UnlockMagicNode speedUpUnlock = new UnlockMagicNode("SpeedUp", speedUp);
			base2.ChildNodes.Add(speedUpUnlock);
			speedUpUnlock.ParentNode = base2;
			//nodes.Add(speedUp);

			LevelUpNode heal = GetMagicBase("Heal");
			UnlockMagicNode healUnlock = new UnlockMagicNode("Heal", heal);
			speedUpUnlock.ChildNodes.Add(healUnlock);
			healUnlock.ParentNode = speedUpUnlock;
			//nodes.Add(heal);

			return nodes;
		}

		private static LevelUpNode GetBaseAbilityForElement(Element element, string magicName)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.NewAbility, 100);
			node.Element = element;
			node.MagicName = magicName;
			node.ChildNodes.Add(GetDamageLevelUp(magicName, node, GetDamageBase(element)));
			node.ChildNodes.Add(GetSpeedLevelUp(magicName, node));
			node.ParentNode = null;
			node.Name = element.ToString() + "\r\nAbility";
			return node;
		}

		private static LevelUpNode GetBaseAbilityForBall(Element element, string magicName)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.NewAbility, 100);
			node.Element = element;
			node.MagicName = magicName;
			node.ChildNodes.Add(GetDamageLevelUp(magicName, node, 6, 2));
			node.ChildNodes.Add(GetSpeedLevelUp(magicName, node, 1));
			node.ParentNode = null;
			node.Name = "Ball\r\nAbility";

			LevelUpNode abilityNodes = GetAddAttributeLevelUp("Ball", AbilityAttributes.Explosion, node);
			LevelUpNode nextAbilityNodes = GetAddAttributeLevelUp("Ball", AbilityAttributes.ClickToDetonate | AbilityAttributes.PassCharacters, abilityNodes);
			nextAbilityNodes.ChildNodes.Add(GetKnockbackLevelUp("Ball", node));
			abilityNodes.ChildNodes.Add(nextAbilityNodes);
			node.ChildNodes.Add(abilityNodes);

			return node;
		}

		private static int GetDamageBase(Element element)
		{
			if (element == Element.Fire)
				return 1;
			else if (element == Element.Lightning)
				return 9;
			else if (element == Element.Rock)
				return 22;
			return 8;
		}

		private static LevelUpNode GetDamageLevelUp(string magicName, LevelUpNode parentNode, int damageBase, int number = 3)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Magic, 320 - ((number * 8) * 10));
			node.MagicName = magicName;
			node.Amount = -((4 - number) * damageBase);
			node.MagicType = LevelUpMagicType.Damage;
			node.ParentNode = parentNode;
			node.Name = string.Format("Damage\r\n+ {0:0.0}", node.Amount);

			if (number > 0)
				node.ChildNodes.Add(GetDamageLevelUp(magicName, node, damageBase, number - 1));

			return node;
		}

		private static LevelUpNode GetAddAttributeLevelUp(string magicName, AbilityAttributes attribute, LevelUpNode parentNode)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Magic, 100);
			node.MagicName = magicName;
			node.Amount = (float)attribute;
			node.MagicType = LevelUpMagicType.NewAbilityAttribute;
			node.ParentNode = parentNode;
			node.Name = string.Format(attribute.ToString());

			return node;
		}

		private static LevelUpNode GetSpeedLevelUp(string magicName, LevelUpNode parentNode, int number = 2)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Magic, 240 - ((number * 6) * 10));
			node.MagicName = magicName;
			node.Amount = (4 - number) / 2.0f;
			node.MagicType = LevelUpMagicType.Speed;
			node.ParentNode = parentNode;
			node.Name = string.Format("Speed\r\n+ {0:0.0}", node.Amount);

			if (number > 0)
				node.ChildNodes.Add(GetSpeedLevelUp(magicName, node, number - 1));

			return node;
		}

		private static LevelUpNode GetStunLevelUp(string magicName, LevelUpNode parentNode, int number = 1)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Magic, 240 - ((number * 6) * 10));
			node.MagicName = magicName;
			node.Amount = 14f;//(4 - number) / 2.0f;
			node.MagicType = LevelUpMagicType.NewCustomAbility;
			node.MagicAbility = LevelUpMagicAbility.Stun;
			node.ParentNode = parentNode;
			node.Name = string.Format("Stun\r\n+ {0:0.0}", node.Amount);

			if (number > 0)
				node.ChildNodes.Add(GetStunLevelUp(magicName, node, number - 1));

			return node;
		}

		private static LevelUpNode GetSlowLevelUp(string magicName, LevelUpNode parentNode, int number = 2)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Magic, 160 - ((number * 6) * 10));
			node.MagicName = magicName;
			node.Amount = .25f;//(4 - number) / 2.0f;
			node.MagicType = LevelUpMagicType.NewCustomAbility;
			node.MagicAbility = LevelUpMagicAbility.Slow;
			node.ParentNode = parentNode;
			node.Name = string.Format("Slow\r\n+ {0:0.0}", node.Amount);

			if (number == 2)
			{
				LevelUpNode node2 = new LevelUpNode(LevelUpType.Magic, 0);
				node2.MagicName = magicName;
				node2.Amount = 4;//(4 - number) / 2.0f;
				node2.MagicType = LevelUpMagicType.NewCustomAbility;
				node2.MagicAbility = LevelUpMagicAbility.SlowTime;

				node.AdditionalNodes.Add(node2);
			}

			if (number > 0)
				node.ChildNodes.Add(GetSlowLevelUp(magicName, node, number - 1));

			return node;
		}

		private static LevelUpNode GetKnockbackLevelUp(string magicName, LevelUpNode parentNode = null, int number = 1)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Magic, 175);
			node.MagicName = magicName;
			node.Amount = 2f + number * 6;//(4 - number) / 2.0f;
			node.MagicType = LevelUpMagicType.NewCustomAbility;
			node.MagicAbility = LevelUpMagicAbility.KnockBack;
			node.ParentNode = parentNode;
			node.Name = string.Format("KnockBack\r\n+ {0:0.0}", node.Amount);

			if (number > 0)
				node.ChildNodes.Add(GetKnockbackLevelUp(magicName, node, number - 1));

			return node;
		}

		private static LevelUpNode GetSelfUpgrades()
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Health, 0);
			node.Name = "Self";
			node.MagicName = "Self";
			node.AlreadyPurchased = true;

			node.ChildNodes.Add(GetHealthUpgrade(node));
			node.ChildNodes.Add(GetRegenUpgrade(node));
			node.ChildNodes.Add(GetSpeedUpgrade(node));

			return node;
		}

		private static LevelUpNode GetMagicBase(string magicName)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.NewAbility, 100);
			node.Element = Element.Normal;
			node.MagicName = magicName;
			node.ParentNode = null;
			node.Name = string.Format(magicName);
			return node;
		}

		private static LevelUpNode GetHealthUpgrade(LevelUpNode parentNode, int number = 3)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Health, 40 * (4 - number));

			node.MagicName = "Self";
			node.Amount = 100 + ((4 - number) * 50);
			node.ParentNode = parentNode;

			node.Name = string.Format("Health\r\n+ {0:0.0}", node.Amount);

			if (number > 0)
				node.ChildNodes.Add(GetHealthUpgrade(node, number - 1));

			return node;
		}

		private static LevelUpNode GetSpeedUpgrade(LevelUpNode parentNode, int number = 3)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Speed, 120 + ((3 - number) * 80));

			node.MagicName = "Self";
			node.Amount = .45f;
			node.ParentNode = parentNode;

			node.Name = string.Format("Speed\r\n+ {0:0.0}", node.Amount);

			if (number > 0)
				node.ChildNodes.Add(GetSpeedUpgrade(node, number - 1));

			return node;
		}

		private static LevelUpNode GetRegenUpgrade(LevelUpNode parentNode, int number = 4)
		{
			LevelUpNode node = new LevelUpNode(LevelUpType.Regen, (int)(Math.Pow(7 - number, 1.65) * 11));

			node.MagicName = "Self";
			node.Amount = .025f + ((4 - number) * .005f);
			node.ParentNode = parentNode;

			node.Name = string.Format("Regen\r\n+ {0:0.0}", (node.Amount * 3600));

			if (number > 0)
				node.ChildNodes.Add(GetRegenUpgrade(node, number - 1));

			return node;
		}
	}
}
