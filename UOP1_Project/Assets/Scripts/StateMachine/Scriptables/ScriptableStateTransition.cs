﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeivSky.StateMachine.Scriptables
{
	[CreateAssetMenu(fileName = "New Transition", menuName = "State Machines/Transition")]
	public class ScriptableStateTransition : ScriptableObject
	{
		[SerializeField] private ScriptableState _targetState = null;
		[SerializeField] private ConditionUsage[] _conditions = default;

		internal StateTransition GetTransition(StateMachine stateMachine, Dictionary<ScriptableObject, object> createdInstances)
		{
			if (createdInstances.TryGetValue(this, out var obj))
				return (StateTransition)obj;

			var state = _targetState.GetState(stateMachine, createdInstances);
			ProcessConditionUsages(stateMachine, _conditions, createdInstances, out var conditions, out var resultGroups);

			var transition = new StateTransition(state, conditions, resultGroups);
			createdInstances.Add(this, transition);
			return transition;
		}

		private static void ProcessConditionUsages(
			StateMachine stateMachine,
			ConditionUsage[] conditionUsages,
			Dictionary<ScriptableObject, object> createdInstances,
			out StateCondition[] conditions,
			out int[] resultGroups)
		{
			int count = conditionUsages.Length;
			conditions = new StateCondition[count];
			for (int i = 0; i < count; i++)
				conditions[i] = conditionUsages[i].Condition.GetCondition(
					stateMachine, conditionUsages[i].ExpectedResult == Result.True, createdInstances);


			List<int> resultGroupsList = new List<int>();
			for (int i = 0; i < count; i++)
			{
				int idx = i;
				resultGroupsList.Add(1);
				while (i < count - 1 && conditionUsages[i].Operator == Operator.And)
				{
					i++;
					resultGroupsList[idx]++;
				}
			}

			resultGroups = resultGroupsList.ToArray();
		}

		[Serializable]
		public struct ConditionUsage
		{
			public Result ExpectedResult;
			public ScriptableStateCondition Condition;
			public Operator Operator;
		}

		public enum Result { True, False }
		public enum Operator { And, Or }
	}
}
