namespace APIsolarMonitoring.Models
{
    public class Datalogger1Filters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool VM4T_E_Irrad_Global_Avg { get; set; }
        public bool VM4T_E_Irrad_Difusa_Avg { get; set; }
        public bool VM4T_E_TAmb_Avg { get; set; }
        public bool VM4T_E_Hum_Rel_Avg { get; set; }
        public bool VM4T_E_P_Rocio_Avg { get; set; }
        public bool VM4T_E_Vel_viento_Avg { get; set; }
        public bool VM4T_E_Dir_viento_Avg { get; set; }
        public bool VM4T_E_P_atm_Avg { get; set; }
        public bool VM4T_E_Precip_Avg { get; set; }
        public bool VM4T_E_Precip_int_Avg { get; set; }
        public bool VM4T_IO_Irrad_RT1_Avg { get; set; }
        public bool VM4T_IO_Temp_RT1_Avg { get; set; }
    }

}
