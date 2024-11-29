namespace Framework.AuthServer.Helpers
{
    public static class LeaveCalculator
    {
        public static double CalculateBusinessDaysDuration(DateTime startTime, DateTime endTime)
        {
            double totalDays = 0;
            DateTime current = startTime;

            while (current.Date <= endTime.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    bool isStartDay = current.Date == startTime.Date;
                    bool isEndDay = current.Date == endTime.Date;

                    DateTime dayStart = isStartDay ? startTime : new DateTime(current.Year, current.Month, current.Day, 8, 0, 0);
                    DateTime dayEnd = isEndDay ? endTime : new DateTime(current.Year, current.Month, current.Day, 17, 0, 0);

                    if (dayStart < dayEnd)
                    {
                        TimeSpan workDayDuration = TimeSpan.Zero;

                        if (dayStart.Hour < 12)
                        {
                            DateTime noon = new DateTime(current.Year, current.Month, current.Day, 12, 0, 0);
                            if (dayEnd <= noon)
                            {
                                workDayDuration += dayEnd - dayStart;
                            }
                            else
                            {
                                workDayDuration += noon - dayStart;
                            }
                        }

                        if (dayEnd.Hour > 12)
                        {
                            DateTime afternoonStart = new DateTime(current.Year, current.Month, current.Day, 13, 0, 0);
                            if (dayStart < afternoonStart)
                            {
                                dayStart = afternoonStart;
                            }
                            if (dayEnd > afternoonStart)
                            {
                                workDayDuration += dayEnd - dayStart;
                            }
                        }

                        totalDays += workDayDuration.TotalHours / 8.0;
                    }
                }

                current = current.AddDays(1);
            }

            return totalDays;
        }

        public static void ValidateAbsenceDate(DateTime date, DateTime employmentDate)
        {
            int hour = date.Hour;
            if(date < employmentDate)
                throw new Exception("StartTime and EndTime must be bigger then your employment date.");

            if (hour != 8 && hour != 12 && hour != 17)
                throw new Exception("StartTime and EndTime must be at 08:00, 12:00, or 17:00.");
        }
    }
}
