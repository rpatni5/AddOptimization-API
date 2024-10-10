using Microsoft.AspNetCore.Http;
using NPOI.HSSF.Record;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CvEntryDataDto
    {
        public List<ExperienceDto> Experience { get; set; } 
        public List<EducationDto> Education { get; set; }
        public List<ProjectDto> Project { get; set; } 
        public List<LanguageDto> Language { get; set; }
        public List<TechnicalKnowledgeDto> TechnicalKnowledge { get; set; } 
        public List<ContactDto> Contact { get; set; } 
        public List<CertificateDto> Certificate { get; set; }
    }

    public class ExperienceDto
    {
        public string Role { get; set; }
        public string Company { get; set; }
        public string CompanyLocation { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyStartDate { get; set; }
        public string CompanyEndDate { get; set; }
    }

    public class EducationDto
    {
        public string Degree { get; set; }
        public string Institution { get; set; }
        public string InstitutionLocation { get; set; }
        public string EducationDescription { get; set; }
        public string EducationStartDate { get; set; }
        public string EducationEndDate { get; set; }
    }

    public class ProjectDto
    {
        public string ProjectTitle { get; set; }
        public string ProjectOrganization { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectStartDate { get; set; }
        public string ProjectEndDate { get; set; }
    }

    public class LanguageDto
    {
        public string Language { get; set; }
        public string LanguageLevel { get; set; }
    }

    public class TechnicalKnowledgeDto
    {
        public string Type { get; set; } 
        public List<string> NameList { get; set; }
    }

    public class ContactDto
    {
        public string Title { get; set; }
        public string FullName { get; set; }
        public string Dob { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class CertificateDto
    {
        public string CertificateName { get; set; }
        public string CertificatePath { get; set; }
        public IFormFile File { get; set; }
    }
}
