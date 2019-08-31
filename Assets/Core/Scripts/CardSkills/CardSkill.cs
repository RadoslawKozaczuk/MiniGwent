using Assets.Core.DataModel;
using System;

namespace Assets.Core.CardSkills
{
    public class CardSkill
    {
        // when
        public readonly CardSkillExecutionMoment ExecutionMoment;
        public readonly CardSkillTarget Target;
        public readonly Action<CardModel> Effect;
        public readonly VisualEffect VisualEffect;

        public CardSkill(CardSkillExecutionMoment executionMoment, CardSkillTarget target, 
            Action<CardModel> effect, VisualEffect visualEffect)
        {
            ExecutionMoment = executionMoment;
            Target = target;
            Effect = effect;
            VisualEffect = visualEffect;
        }
    }
}
