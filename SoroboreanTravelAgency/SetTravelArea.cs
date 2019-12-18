using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using NodeCanvas.Tasks.Actions;
//using ODebug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static AreaManager;

namespace OutwardMods
{
    public class SetTravelArea : ActionTask
    {
        public SoroboreanTravelAgency Script { get; set; }
        public AreaEnum TargetArea { get; set; }

        protected override void OnExecute()
        {
            try
            {
                Script.m_travelArea = (int) TargetArea;
            }
            catch (Exception ex)
            {
                //OLogger.Error(ex.Message);
                Debug.Log($"[SoroboreanTravelAgency] SetTravelArea.OnExecute: {ex.Message}");
            }
            EndAction();
        }
    }
}
