using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;
using RimWorld.Planet;

namespace Dragonian
{/*
    public class Apparel_Draft:Apparel
	{
		public static Dictionary<Def, Graphic> graphicDict = new Dictionary<Def, Graphic>();
		//private Graphic fullGraphic = GraphicDatabase.Get<Graphic_Multi>("Apparel/" + "DR_BattleDress"+"Full", ShaderDatabase.CutoutComplex, new Vector2(1, 1), new Color(1, 1, 1, 1));
		static readonly Vector3 drawDraftedLoc = new Vector3(0, 0.0285151523f, 0);
		public static readonly Vector2 drawSize = new Vector2(1.5f, 1.5f);

		public override void PostMake()
		{
			base.PostMake(); 
			if (!graphicDict.ContainsKey(def))
			{
				graphicDict.Add(def, GraphicDatabase.Get<Graphic_Multi>("Apparel/" + def.defName + "Full", ShaderDatabase.CutoutComplex));
			}
		}
		public override void ExposeData()
		{
			base.ExposeData();
			LongEventHandler.ExecuteWhenFinished(delegate {
				if (!graphicDict.ContainsKey(def))
				{
					Pawn pawn = Wearer;
					graphicDict.Add(def, GraphicDatabase.Get<Graphic_Multi>("Apparel/" + def.defName + "Full", ShaderDatabase.CutoutComplex));
				}
			});
		}

		private bool ShouldDisplay
		{
			get
			{
				Pawn wearer = Wearer;
				return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner));
			}
		}
		public override void DrawWornExtras()
		{
			if(ShouldDisplay)
			{
				Pawn pawn = Wearer;
				Vector3 rootLoc = pawn.DrawPos + drawDraftedLoc;
				Mesh mesh = MeshPool.GridPlane(drawSize);
				Graphics.DrawMesh(mesh, rootLoc, Quaternion.AngleAxis(0, Vector3.up), graphicDict[def].MatAt(pawn.Rotation), 0);
			}
		}
	}*/
}
