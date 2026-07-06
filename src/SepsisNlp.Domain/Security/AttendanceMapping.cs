using SepsisNlp.Domain.Common;

namespace SepsisNlp.Domain.Security;

public class AttendanceMapping : Entity
{
	public Guid AttendanceId { get; private set; }
	public string RealAttendanceNumber { get; private set; }

	public AttendanceMapping() { }

	public AttendanceMapping(Guid attendanceId, string realAttendanceNumber)
	{
		AttendanceId = attendanceId;
		RealAttendanceNumber = realAttendanceNumber;
	}
}