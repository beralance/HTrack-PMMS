using System.ComponentModel.DataAnnotations;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Features.Drafts.SaveDraft;

public static class SaveDraftBehavior {
    public static void ValidateDraft(Draft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.ProjectName))
        {
            throw new ValidationException(
                "Project name is required.");
        }

        if (draft.DateIssued == default)
        {
            throw new ValidationException(
                "Date issued is required.");
        }

        if (draft.MunicipalityId <= 0)
        {
            throw new ValidationException(
                "Municipality is required.");
        }

        if (draft.ProjectTypeId <= 0)
        {
            throw new ValidationException(
                "Project type is required.");
        }
    }

    public static ProjectStatuses DetermineProjectStatus(Draft draft)
    {
        if (draft.HasCoc == true && draft.HasDod == true)
        {
            return ProjectStatuses.FullyCompleted;
        }

        if (draft.HasCoc == true)
        {
            return ProjectStatuses.Completed;
        }

        if (draft.ExtensionOfTime is not null)
        {
            return ProjectStatuses.Extended;
        }

        return ProjectStatuses.OnGoing;
    }

    public static Project MapProject(
        Draft draft,
        ProjectStatuses status,
        string userId,
        DateTimeOffset now)
    {
        return new Project
        {
            DateIssued = draft.DateIssued,
            ProjectName = draft.ProjectName.Trim(),
            Developer = draft.Developer,
            Owner = draft.Owner,
            CrNo = draft.CrNo,
            LsNo = draft.LsNo,
            Barangay = draft.Barangay,
            Salable = draft.Salable,
            IsFm = draft.IsFm,

            HasCoc = draft.HasCoc,
            HasDod = draft.HasDod,
            CocDate = draft.CocDate,
            DodDate = draft.DodDate,

            Status = status,
            Remarks = draft.Remarks,

            SetupStatus = ProjectSetupStatuses.Active,
            IsPublished = true,
            IsExtended = draft.ExtensionOfTime.HasValue,

            MunicipalityId = draft.MunicipalityId,
            ProjectTypeId = draft.ProjectTypeId,

            CreatedAt = now,
            AddedById = userId
        };
    }
}