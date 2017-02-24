using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn;
using RedArrow.Compass.CareTeam.CaseManagement.Model.Forms;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model
{

    /*
     * Additional notes from Drew, on the patient app:
     * 
           for every stage, there will be 2 milestones 

            each milestone will be named in the format of:

            stage.{stage name}.started
            stage.{stage name}.ended

            the possible status/state values of each stage are:

            * completed
            * available

            So, given enrollment has started (is in progress) for a patient, then milestones would be:

            stage.enrollment.started -> State.Completed
            stage.enrollment.ended -> State.Available

            Given enrollment has been finished for a patient, the milestones would be:

            stage.enrollment.started -> State.Completed
            stage.enrollment.ended -> State.Completed

            As such, the necessary milestone names are:

            * stage.enrollment.started
            * stage.enrollment.ended
            * stage.onboarding.started
            * stage.onboarding.ended
            * stage.benefitt-investigation.started
            * stage.benefitt-investigation.ended
            * stage.assistance-program.started
            * stage.assistance-program.ended
            * stage.appeal.started
            * stage.appeal.ended
            * stage.shipment.started
            * stage.shipment.ended


            The criteria for each stage’s stage change is determined here: https://docs.google.com/document/d/1e8VMP3Zo19rx1KUg8nekAQy9JcEiAH-BCG61WX1JiE8/edit
            Reference the “Patient-focused” status table.
     * 
     */

    [Model(TITAN_TYPE)]
    public class CompassCaseDefinition
    {
        public const string TITAN_TYPE = "compass-case-definition";

        public const string COMPASS_CASE_TYPE = "compass-case";
        public const string COMPASS_CASE_TITLE = "Compass Case";

        [Id]
        public Guid Id { get; set; }

        [HasMany]
        public ICollection<EntryPointDefinition> EntryPoints { get; set; }

        //public static CompassCaseDefinition NewDefinition()
        //{
        //    var caseFileItem = new CompassCaseFileItem();
        //    var instance = new CompassCaseDefinition()
        //    {
        //        EntryPoints = new List<EntryPointDefinition>()
        //        {
        //            new EntryPointDefinition()
        //            {
        //                Id = Guid.Parse("E969CBC1-4723-45F4-91D6-6FFD4909B50B"),
        //                CaseType = COMPASS_CASE_TYPE,
        //                Title = COMPASS_CASE_TITLE,
        //                Roles = new List<Role>()
        //                {
        //                    new Role() {Name = "CareTeam"}
        //                },
        //                OutputMappings = new List<CompassMapping>()
        //                {
        //                    new CompassMapping()
        //                    {
        //                        CaseFileItem = caseFileItem,
        //                        CaseFileItemProperty = "$.Demographics.FirstName",
        //                        TaskVariableName = "first-name"
        //                    },
        //                    new CompassMapping()
        //                    {
        //                        CaseFileItem = caseFileItem,
        //                        CaseFileItemProperty = "$.Demographics.LastName",
        //                        TaskVariableName = "last-name"
        //                    },
        //                    new CompassMapping()
        //                    {
        //                        CaseFileItem = caseFileItem,
        //                        CaseFileItemProperty = "$.Demographics.Email",
        //                        TaskVariableName = "email"
        //                    }
        //                },
        //                FormDef = BuildSimpleFormDef()
        //            }
        //        }
        //    };
        //    return instance; 
        //}

        private static FormDefinitionEntity BuildSimpleFormDef()
        {
            return new FormDefinitionEntity()
            {
                Id = Guid.Parse("07f45f3f-f61d-46a9-a3f0-dd6d12069971"),
                Name = "care-team-enrollment-form",
                Title = "Enrollment Form",
                Sections = new List<FormSectionEntity>()
                {
                    new FormSectionEntity()
                    {
                        Title = "Patient Demographics",
                        FieldGroups = new List<FieldGroupEntity>()
                        {
                            new FieldGroupEntity()
                            {
                                Title = "",
                                AddMany = false,
                                Fields = new List<FieldEntity>()
                                {
                                    new FieldEntity()
                                    {
                                        Name = "first-name",    // same as Binding
                                        Type = "text",
                                        DefaultValue = "",
                                        DisplayName = "First Name",
                                        Description = "Patient first name",
                                        Options = new SelectOptionEntity[0]
                                    },
                                    new FieldEntity()
                                    {
                                        Name = "last-name",    // same as Binding
                                        Type = "text",
                                        DefaultValue = "",
                                        DisplayName = "Last Name",
                                        Description = "Patient last name",
                                        Options = new SelectOptionEntity[0]
                                    },
                                    new FieldEntity()
                                    {
                                        Name = "email",    // same as Binding
                                        Type = "text",
                                        DefaultValue = "",
                                        DisplayName = "Email",
                                        Description = "Patient email",
                                        Options = new SelectOptionEntity[0]
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static CompassCase NewCaseInstance()
        {
            var instance = new CompassCase()
            {
                Name = "Compass Case Definition",
            };

            var patientCaseFileItem = new CompassCaseFileItem()
            {
                State = CmmnState.Enabled,
                Uri = null,
                FormDefinition = BuildSimpleFormDef()
            };

            // Case File Model
            instance.CaseFileModel = new CompassCaseFile()
            {
                CaseFileItems = new List<CompassCaseFileItem>()
                {
                    patientCaseFileItem
                }
            };

            
            // Case Stage
            var enrollmentStage = new CompassStage()
            {
                Name = "enrollment",
                DisplayName = "Patient Enrollment Stage",
                State = CmmnState.Enabled,
                Tasks = new List<CompassTask>()
            };
            var onboarding = new CompassStage()
            {
                Name = "onboarding",
                DisplayName = "onboarding",
                State = CmmnState.Enabled,
                Tasks = new List<CompassTask>()
            };
            var benefit = new CompassStage()
            {
                Name = "benefit",
                DisplayName = "benefit",
                State = CmmnState.Enabled,
                Tasks = new List<CompassTask>()
            };
            var assistance = new CompassStage()
            {
                Name = "assistance",
                DisplayName = "assistance",
                State = CmmnState.Enabled,
                Tasks = new List<CompassTask>()
            };
            var appeal = new CompassStage()
            {
                Name = "appeal",
                DisplayName = "appeal",
                State = CmmnState.Enabled,
                Tasks = new List<CompassTask>()
            };
            var shipment = new CompassStage()
            {
                Name = "shipment",
                DisplayName = "shipment",
                State = CmmnState.Enabled,
                Tasks = new List<CompassTask>()
            };

            // Case Plan Model
            instance.CasePlanModel = new CompassCasePlanModel
            {
                State = CmmnState.Enabled,
                Stages = new List<CompassStage>
                {
                    enrollmentStage,
                    onboarding,
                    benefit,
                    shipment,
                    appeal,
                    assistance
                    //benefit
                },
                Milestones = null
            };

            return instance;
        }
    }
}