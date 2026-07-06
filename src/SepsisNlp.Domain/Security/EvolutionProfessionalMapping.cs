using SepsisNlp.Domain.Common;

namespace SepsisNlp.Domain.Security;

public class EvolutionProfessionalMapping : Entity
{
	public string OriginalEvolutionCode { get; private set; }
	public string ProfessionalName { get; private set; }
	public string ProfessionalCouncil { get; private set; }

	protected EvolutionProfessionalMapping() { }

	public EvolutionProfessionalMapping(string originalEvolutionCode, string professionalName, string professionalCouncil)
	{
		OriginalEvolutionCode = originalEvolutionCode;
		ProfessionalName = professionalName;
		ProfessionalCouncil = professionalCouncil;
	}
}