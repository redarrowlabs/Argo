using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    /// <summary>
    /// A Task is an atomic unit of work.
    /// </summary>
    public interface Task : PlanItemDefinition
    {
        /// <summary>
        /// If isBlocking is set to TRUE, the Task is waiting until the work associated with the Task is completed.
        /// If isBlocking is set to FALSE, the Task is not waiting for the work to complete and completes immediately, upon instantiation.
        /// The default value of attribute isBlocking MUST be TRUE.
        /// A Task that is non-blocking (isBlocking set to FALSE) MUST NOT have outputs.
        /// </summary>
        bool IsBlocking { get; set; }

        /// <summary>
        /// Zero or more CaseParameter objects (see 5.4.10.3) that specify the input of the Task.
        /// </summary>
        ICollection<InputCaseParameter> Inputs { get; set; }

        /// <summary>
        /// Zero or more CaseParameter objects (see 5.4.10.3) that specify the output of the Task.
        /// </summary>
        ICollection<OutputCaseParameter> Outputs { get; set; }
    }

    /// <summary>
    /// A HumanTask is a Task that is performed by a Case worker.
    /// When a HumanTask is not “blocking” (isBlocking is FALSE), it can be considered a “manual” Task, i.e., the Case management system is not tracking the lifecycle of the HumanTask(instance).
    /// </summary>
    [Model("case-task-human")]
    public class HumanTask : Task
    {
        [Id]
        public Guid Id { get; set; }

        /// <summary>
        /// An optional PlanningTable associated to the HumanTask. A HumanTask can be used for planning, and its PlanningTable might contain TableItems that are useful in the particular planning context.
        /// A HumanTask that is non-blocking (isBlocking set to FALSE) MUST NOT have a PlanningTable.
        /// </summary>
        [HasOne]
        public PlanningTable PlanningTable { get; set; }

        /// <summary>
        /// The performer of the HumanTask.
        /// </summary>
        [HasOne]
        public Role Performer { get; set; }

        [Property]
        public string Name { get; set; }
        [HasOne]
        public DefaultControl DefaultControl { get; set; }

        [Property]
        public bool IsBlocking { get; set; } = true;
        [HasMany]
        public ICollection<InputCaseParameter> Inputs { get; set; }
        [HasMany]
        public ICollection<OutputCaseParameter> Outputs { get; set; }
    }

    /// <summary>
    /// A ProcessTask can be used in the Case to call a Business Process.
    /// Parameters are used to pass information between the ProcessTask (in a Case) and the Process to which it refers:
    /// inputs of the ProcessTask are mapped to Inputs of the Process, and outputs of the ProcessTask are mapped to outputs of the Process.
    /// 
    /// When a ProcessTask is “blocking” (isBlocking is TRUE), the ProcessTask is waiting until the Process associated with the ProcessTask is completed.
    /// If isBlocking is set to FALSE, the ProcessTask is not waiting for the Process to complete, and completes immediately upon its instantiation and calling its associated Process.
    /// </summary>
    [Model("case-task-process")]
    public class ProcessTask : Task
    {
        //TODO
        [Id]
        public Guid Id { get; set; }
        [Property]
        public string Name { get; set; }
        [HasOne]
        public DefaultControl DefaultControl { get; set; }
        [Property]
        public bool IsBlocking { get; set; }
        [HasMany]
        public ICollection<InputCaseParameter> Inputs { get; set; }
        [HasMany]
        public ICollection<OutputCaseParameter> Outputs { get; set; }
    }
}
