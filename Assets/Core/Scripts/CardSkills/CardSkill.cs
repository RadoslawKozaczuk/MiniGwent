using Assets.Core.DataModel;

namespace Assets.Core.CardSkills
{
    public class CardSkill
    {
        // when
        public CardSkillExecutionMoment ExecutionMoment;
        public CardSkillExecutionControlType ControlType;

        public CardSkill(CardSkillExecutionMoment executionMoment, CardSkillExecutionControlType controlType)
        {
            ExecutionMoment = executionMoment;
            ControlType = controlType;
        }

        // what to do
        // for now a simplistic version
        // add +1 strength to the ally on your right (if you are the last one then nothing)
        public void Execute(CardModel target)
        {
            target.CurrentStrength += 1;
        }
    }
}
