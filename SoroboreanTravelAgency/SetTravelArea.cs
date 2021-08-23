using NodeCanvas.Framework;
using System;
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
                SoroboreanTravelAgency.TravelArea = (int) TargetArea;
            }
            catch (Exception ex)
            {
                SoroboreanTravelAgency.Instance.MyLogger.LogError(ex.Message);
            }
            EndAction();
        }
    }
}
