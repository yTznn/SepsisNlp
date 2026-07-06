using SepsisNlp.Domain.Common;
using SepsisNlp.Domain.Entities;

namespace SepsisNlp.Domain.Security;

public class PatientMapping : Entity
{
    public Guid PatientId { get; private set; }
    public string RealMedicalRecord { get; private set; }
    public string RealName { get; private set; }

    public virtual Patient Patient { get; private set; }

    protected PatientMapping() { }

    public PatientMapping(Guid patientId, string realMedicalRecord, string realName)
    {
        PatientId = patientId;
        RealMedicalRecord = realMedicalRecord;
        RealName = realName;
    }
}