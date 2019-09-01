using Assets.Core.DataModel;
using System;
using System.Collections.Generic;

namespace Assets.Core.CardSkills
{
    public class CardSkill
    {
        public readonly SkillExecutionTime ExecutionTime;
        public readonly List<SkillTarget> Targets;
        public readonly Action<CardModel> Effect;
        public readonly SkillVisualEffect VisualEffect;

        public CardSkill(SkillExecutionTime executionTime, List<SkillTarget> targets, Action<CardModel> effect, SkillVisualEffect visualEffect)
        {
#if UNITY_EDITOR
            if (targets == null)
                throw new ArgumentNullException("targets", "Targets parameter cannot be null.");
            else if (targets.Count == 0)
                throw new ArgumentException("Targets parameter cannot be empty.", "targets");

            if (effect == null)
                throw new ArgumentNullException("effect", "Effect parameter cannot be null.");
#endif

            ExecutionTime = executionTime;
            Targets = targets;
            Effect = effect;
            VisualEffect = visualEffect;
        }
    }
}
