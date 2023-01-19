using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Utils; 

public static class AssignmentUtils {

    public const int MaxAssignmentNameLength = 64;
    public const int MinAssignmentNameLength = 2;

    public const int MaxAssignmentNoteLength = 1024;
    public const int MinAssignmentNoteLength = 1;

    public static EAssignmentCreationResponse ValidateNewAssignment(Assignment assignment, Project? project, UserBase? user) {

        if (project == null || assignment.ProjectId != project.Id) {
            return EAssignmentCreationResponse.InvalidProject;
        }

        if (user == null || assignment.UserId != user.Id) {
            return EAssignmentCreationResponse.InvalidUser;
        }

        if (assignment.Name.Length is < MinAssignmentNameLength or > MaxAssignmentNameLength){
            return EAssignmentCreationResponse.InvalidName;
        }
        
        if (assignment.Note.Length < MinAssignmentNoteLength && assignment.Note.Length > MaxAssignmentNoteLength) {
            return EAssignmentCreationResponse.InvalidNote;
        }

        var timeDiff = DateTime.Compare(project.FromDate, assignment.FromDate);
        if (timeDiff > 0) {
            //assignment earlier than the project start date! this is not allowed!
            return EAssignmentCreationResponse.InvalidFromDate;
        }
        
        timeDiff = DateTime.Compare(assignment.FromDate, assignment.ToDate);
        if (timeDiff >= 0){
            //toDate is earlier or from the same time as fromDate
            return EAssignmentCreationResponse.InvalidToDate;
        }
        if (assignment.AllocationScope < 1) {
            return EAssignmentCreationResponse.InvalidAllocationScope;
        }

        return EAssignmentCreationResponse.Ok;
    }
    
    public static EWorkloadValidation ValidateWorkloadPayload(long[] ids, DateTime? fromDate, DateTime? toDate) {
        if (ids.Length == 0) {
            return EWorkloadValidation.EmptyUsers;
        }

        if (ids.Any(x => x < 1)) {
            return EWorkloadValidation.InvalidUserId;
        }

        return ValidateWorkloadPayload(fromDate, toDate);
    }

    public static EWorkloadValidation ValidateWorkloadPayload(string[] ids, DateTime? fromDate, DateTime? toDate) {
        if (ids.Length < 1) {
            return EWorkloadValidation.EmptyUsers;
        }

        return ValidateWorkloadPayload(fromDate, toDate);
    }

    private static EWorkloadValidation ValidateWorkloadPayload(DateTime? fromDate, DateTime? toDate) {
        
        if (fromDate == null) {
            return EWorkloadValidation.InvalidFromDate;
        }

        if (toDate == null) {
            return EWorkloadValidation.InvalidToDate;
        }

        var timeDiff = DateTime.Compare(ProjectUtils.RefDateTime, (DateTime)fromDate);
        if (timeDiff > 0) {
            //earlier from date than allowed!
            return EWorkloadValidation.InvalidFromDate;
        }
        timeDiff = DateTime.Compare((DateTime)fromDate, (DateTime)toDate);
        if (timeDiff >= 0){
            //toDate is earlier or from the same time as fromDate
            return EWorkloadValidation.InvalidToDate;
        }

        var scope = toDate - fromDate;
        if (scope?.Days > 60) {
            return EWorkloadValidation.TooLongScope;
        }

        return EWorkloadValidation.Ok;
    }

    public static IEnumerable<DateTime> Split(DateTime start, DateTime end) {
        DateTime chunkEnd;
        while ((chunkEnd = start.AddDays(1)) < end) {
            yield return start;
            start = chunkEnd.Date;
        }
        yield return start;
    }

    /// <summary>
    /// Calculates number of business days, taking into account:
    ///  - weekends (Saturdays and Sundays)
    ///  - bank holidays in the middle of the week
    /// </summary>
    /// <param name="firstDay">First day in the time interval</param>
    /// <param name="lastDay">Last day in the time interval</param>
    /// <param name="bankHolidays">List of bank holidays excluding weekends</param>
    /// <returns>Number of business days during the 'span'</returns>
    public static int BusinessDaysUntil(this DateTime firstDay, DateTime lastDay) {
        firstDay = firstDay.Date;
        lastDay = lastDay.Date;
        if (firstDay > lastDay)
            throw new ArgumentException("Incorrect last day " + lastDay);

        TimeSpan span = lastDay - firstDay;
        var businessDays = span.Days + 1;
        var fullWeekCount = businessDays / 7;
        // find out if there are weekends during the time exceedng the full weeks
        if (businessDays > fullWeekCount * 7) {
            // we are here to find out if there is a 1-day or 2-days weekend
            // in the time interval remaining after subtracting the complete weeks
            var firstDayOfWeek = firstDay.DayOfWeek == DayOfWeek.Sunday
                ? 7 : (int)firstDay.DayOfWeek;
            var lastDayOfWeek = lastDay.DayOfWeek == DayOfWeek.Sunday
                ? 7 : (int)lastDay.DayOfWeek;
            if (lastDayOfWeek < firstDayOfWeek)
                lastDayOfWeek += 7;
            if (firstDayOfWeek <= 6) {
                if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                    businessDays -= 2;
                else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                    businessDays -= 1;
            }
            else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                businessDays -= 1;
        }

        // subtract the weekends during the full weeks in the interval
        businessDays -= fullWeekCount + fullWeekCount;
        

        return businessDays;
    }


    public static int GetDayIndex(DateTime[] days, DateTime desiredDate) {
        for (var i = 0; i < days.Length; i++) {
            var timeDiff = DateTime.Compare(days[i].Date, desiredDate.Date);
            if (timeDiff >= 0) {
                //from date is earlier or from the same time
                return i;
            }
        }

        return -1;
    }

}