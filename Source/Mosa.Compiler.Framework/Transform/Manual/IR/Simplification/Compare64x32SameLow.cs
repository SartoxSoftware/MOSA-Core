// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Transform.Manual.IR.Simplification
{
	/// <summary>
	/// Add64MultipleWithCommon
	/// </summary>
	public sealed class Compare64x32SameLow : BaseTransformation
	{
		public Compare64x32SameLow() : base(IRInstruction.Compare64x32)
		{
		}

		public override bool Match(Context context, TransformContext transformContext)
		{
			if (!context.Operand1.IsVirtualRegister)
				return false;

			if (!context.Operand2.IsVirtualRegister)
				return false;

			if (context.Operand1.Definitions.Count != 1)
				return false;

			if (context.Operand1.Definitions[0].Instruction != IRInstruction.To64)
				return false;

			if (!context.Operand1.Definitions[0].Operand1.IsConstant)
				return false;

			if (context.Operand1.Definitions[0].Operand1.ConstantUnsigned32 != 0)
				return false;

			if (context.Operand2.Definitions.Count != 1)
				return false;

			if (context.Operand2.Definitions[0].Instruction != IRInstruction.To64)
				return false;

			if (!context.Operand2.Definitions[0].Operand1.IsConstant)
				return false;

			if (context.Operand2.Definitions[0].Operand1.ConstantUnsigned32 != 0)
				return false;

			return true;
		}

		public override void Transform(Context context, TransformContext transformContext)
		{
			var operand1 = context.Operand1.Definitions[0].Operand2;
			var operand2 = context.Operand2.Definitions[0].Operand2;

			context.SetInstruction(IRInstruction.Compare32x32, context.ConditionCode, context.Result, operand1, operand2);
		}
	}
}
