using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Utils;

public static class AssignmentUtils {
    /// <summary>
    ///     Validation constants for assignment max and min name length
    /// </summary>
    public const int MaxAssignmentNameLength = 64;

    public const int MinAssignmentNameLength = 2;

    /// <summary>
    ///     Validation constants for assignment assignment note max and min length
    /// </summary>
    public const int MaxAssignmentNoteLength = 1024;

    public const int MinAssignmentNoteLength = 1;

    /// <summary>
    ///     Max space between two dates. Validation for workload scope
    /// </summary>
    public const int MaxTimeSpaceBetweenDates = 60;

    /// <summary>
    ///     Validates the passed assignment if it contains all required variables and of needed quality
    /// </summary>
    /// <param name="assignment">Assignment that needs to be validated</param>
    /// <param name="project">Project that this assignment is part of</param>
    /// <param name="user">User that this assignment is assigned to</param>
    /// <returns>Ok if valid, otherwise EAssignmentCreationResponse with error</returns>
    public static AssignmentCreationResponse ValidateNewAssignment(AssignmentPL assignment, ProjectPL? project,
        UserBasePL? user) {
        if (project == null || assignment.ProjectId != project.Id) {
            return AssignmentCreationResponse.InvalidProject;
        }

        if (user == null || assignment.UserId != user.Id) {
            return AssignmentCreationResponse.InvalidUser;
        }

        if (assignment.Name.Length is < MinAssignmentNameLength or > MaxAssignmentNameLength) {
            return AssignmentCreationResponse.InvalidName;
        }

        if (assignment.Note.Length < MinAssignmentNoteLength && assignment.Note.Length > MaxAssignmentNoteLength) {
            return AssignmentCreationResponse.InvalidNote;
        }

        var timeDiff = DateTime.Compare(project.FromDate, assignment.FromDate);
        if (timeDiff > 0) {
            //assignment earlier than the project start date! this is not allowed!
            return AssignmentCreationResponse.InvalidFromDate;
        }

        timeDiff = DateTime.Compare(assignment.FromDate, assignment.ToDate);
        if (timeDiff >= 0) {
            //toDate is earlier or from the same time as fromDate
            return AssignmentCreationResponse.InvalidToDate;
        }

        if (assignment.AllocationScope < 1) {
            return AssignmentCreationResponse.InvalidAllocationScope;
        }

        return AssignmentCreationResponse.Ok;
    }

    /// <summary>
    ///     Validates if the passed user ids are valid and if the dates are in correct format
    /// </summary>
    /// <param name="ids">selected user ids</param>
    /// <param name="fromDate">from date for the assignment start</param>
    /// <param name="toDate">to date for the assignment end</param>
    /// <returns>Ok if valid, otherwise EWorkloadValidation with error</returns>
    public static WorkloadValidation ValidateWorkloadRequest(long[] ids, DateTime? fromDate, DateTime? toDate) {
        if (ids.Length == 0) {
            return WorkloadValidation.EmptyUsers;
        }

        if (ids.Any(x => x < 1)) {
            return WorkloadValidation.InvalidUserId;
        }

        return ValidateWorkloadPayload(fromDate, toDate);
    }

    /// <summary>
    ///     Validates if the passed user ids are valid and if the dates are in correct format
    /// </summary>
    /// <param name="ids">selected user ids</param>
    /// <param name="fromDate">from date for the assignment start</param>
    /// <param name="toDate">to date for the assignment end</param>
    /// <returns>Ok if valid, otherwise EWorkloadValidation with error</returns>
    public static WorkloadValidation ValidateWorkloadPayload(string[] ids, DateTime? fromDate, DateTime? toDate) {
        if (ids.Length < 1) {
            return WorkloadValidation.EmptyUsers;
        }

        return ValidateWorkloadPayload(fromDate, toDate);
    }

    /// <summary>
    ///     Validates if the dates are in correct format and the space gap between them is under 60 days
    /// </summary>
    /// <param name="fromDate">from date for the assignment start</param>
    /// <param name="toDate">to date for the assignment end</param>
    /// <returns>Ok if valid, otherwise EWorkloadValidation with error</returns>
    private static WorkloadValidation ValidateWorkloadPayload(DateTime? fromDate, DateTime? toDate) {
        if (fromDate == null) {
            return WorkloadValidation.InvalidFromDate;
        }

        if (toDate == null) {
            return WorkloadValidation.InvalidToDate;
        }

        var timeDiff = DateTime.Compare(ProjectUtils.RefDateTime, (DateTime)fromDate);
        if (timeDiff > 0) {
            //earlier from date than allowed!
            return WorkloadValidation.InvalidFromDate;
        }

        timeDiff = DateTime.Compare((DateTime)fromDate, (DateTime)toDate);
        if (timeDiff >= 0) {
            //toDate is earlier or from the same time as fromDate
            return WorkloadValidation.InvalidToDate;
        }

        var scope = toDate - fromDate;
        if (scope.Value.Days > MaxTimeSpaceBetweenDates) {
            return WorkloadValidation.TooLongScope;
        }

        return WorkloadValidation.Ok;
    }

    /// <summary>
    ///     Method creates array of all days that are between start date and end date
    /// </summary>
    /// <param name="start">the first date you want in the array</param>
    /// <param name="end">the last date you want in the array</param>
    /// <returns>array of all dates between two dates</returns>
    public static IEnumerable<DateTime> Split(DateTime start, DateTime end) {
        DateTime chunkEnd;
        while ((chunkEnd = start.AddDays(1)) < end) {
            yield return start;
            start = chunkEnd.Date;
        }

        yield return start;
    }

    /// <summary>
    ///     Calculates number of business days, taking into account:
    ///     - weekends (Saturdays and Sundays)
    ///     - bank holidays in the middle of the week
    /// </summary>
    /// <param name="firstDay">First day in the time interval</param>
    /// <param name="lastDay">Last day in the time interval</param>
    /// <returns>Number of business days during the 'span'</returns>
    public static int BusinessDaysUntil(this DateTime firstDay, DateTime lastDay) {
        firstDay = firstDay.Date;
        lastDay = lastDay.Date;
        if (firstDay > lastDay) {
            throw new ArgumentException("Incorrect last day " + lastDay);
        }

        var span = lastDay - firstDay;
        var businessDays = span.Days + 1;
        var fullWeekCount = businessDays / 7;
        // find out if there are weekends during the time exceedng the full weeks
        if (businessDays > fullWeekCount * 7) {
            // we are here to find out if there is a 1-day or 2-days weekend
            // in the time interval remaining after subtracting the complete weeks
            var firstDayOfWeek = firstDay.DayOfWeek == DayOfWeek.Sunday
                ? 7
                : (int)firstDay.DayOfWeek;
            var lastDayOfWeek = lastDay.DayOfWeek == DayOfWeek.Sunday
                ? 7
                : (int)lastDay.DayOfWeek;
            if (lastDayOfWeek < firstDayOfWeek) {
                lastDayOfWeek += 7;
            }

            if (firstDayOfWeek <= 6) {
                if (lastDayOfWeek >= 7) // Both Saturday and Sunday are in the remaining time interval
                {
                    businessDays -= 2;
                }
                else if (lastDayOfWeek >= 6) // Only Saturday is in the remaining time interval
                {
                    businessDays -= 1;
                }
            }
            else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7) // Only Sunday is in the remaining time interval
            {
                businessDays -= 1;
            }
        }

        // subtract the weekends during the full weeks in the interval
        businessDays -= fullWeekCount + fullWeekCount;


        return businessDays;
    }

    /// <summary>
    ///     Gets the index from the days array that is earlier than the desired date
    /// </summary>
    /// <param name="days">array of days you want to find index in</param>
    /// <param name="desiredDate">date for which you want to find index inside of array</param>
    /// <returns>index of days array that the desired date could be placed in</returns>
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