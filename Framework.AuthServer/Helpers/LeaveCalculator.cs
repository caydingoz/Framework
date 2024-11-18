namespace Framework.AuthServer.Helpers
{
    public static class LeaveCalculator
    {
        public static double CalculateDuration(DateTime startTime, DateTime endTime)
        {
            double totalDays = 0;
            DateTime current = startTime;

            while (current.Date <= endTime.Date)
            {
                bool isStartDay = current.Date == startTime.Date;
                bool isEndDay = current.Date == endTime.Date;

                DateTime dayStart = isStartDay ? startTime : new DateTime(current.Year, current.Month, current.Day, 8, 0, 0);
                DateTime dayEnd = isEndDay ? endTime : new DateTime(current.Year, current.Month, current.Day, 17, 0, 0);

                if (dayStart.Hour == 8 && dayEnd.Hour == 12)
                    totalDays += 0.5;
                else if (dayStart.Hour == 12 && dayEnd.Hour == 17)
                    totalDays += 0.5;
                else if (dayStart.Hour == 8 && dayEnd.Hour == 17)
                    totalDays += 1;

                current = current.AddDays(1);
            }

            return totalDays;
        }
        public static void ValidateAbsenceTime(DateTime time)
        {
            int hour = time.Hour;
            if (hour != 8 && hour != 12 && hour != 17)
            {
                throw new Exception("StartTime and EndTime must be at 08:00, 12:00, or 17:00.");
            }
        }
    }
}
