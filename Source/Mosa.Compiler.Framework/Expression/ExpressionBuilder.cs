﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Common.Exceptions;
using System;
using System.Collections.Generic;

namespace Mosa.Compiler.Framework.Expression
{
	public class ExpressionBuilder
	{
		protected Dictionary<string, BaseInstruction> instructionMap = new Dictionary<string, BaseInstruction>();
		protected Dictionary<string, PhysicalRegister> physicalRegisterMap = new Dictionary<string, PhysicalRegister>();

		public ExpressionBuilder()
		{
		}

		public void AddInstructions(Dictionary<string, BaseInstruction> map)
		{
			foreach (var entry in map)
			{
				instructionMap.Add(entry.Key, entry.Value);
			}
		}

		public void AddPhysicalRegisters(Dictionary<string, PhysicalRegister> map)
		{
			foreach (var entry in map)
			{
				physicalRegisterMap.Add(entry.Key, entry.Value);
			}
		}

		public void AddPhysicalRegisters(PhysicalRegister[] registers)
		{
			foreach (var entry in registers)
			{
				physicalRegisterMap.Add(entry.ToString(), entry);
			}
		}

		public TransformRule CreateExpressionTree(string expression)
		{
			var tokenized = new Tokenizer(expression);

			int transformEnd = tokenized.Tokens.Count - 1;
			int matchEnd = tokenized.Tokens.Count - 1;

			int transformPosition = tokenized.FindFirst(TokenType.Transform);
			int andPosition = tokenized.FindFirst(TokenType.And);

			if (transformPosition != -1)
				matchEnd = transformPosition - 1;

			if (andPosition != -1 && andPosition < matchEnd)
				matchEnd = andPosition - 1;

			var matchTokens = tokenized.GetPart(1, matchEnd);
			var transformTokens = tokenized.GetPart(transformPosition + 1, transformEnd);
			var criteriaTokens = tokenized.GetPart(matchEnd + 1, transformPosition - 1);

			var match = StartParse(matchTokens);
			var transform = StartParse(transformTokens);

			var criteria = ExpressionParser.Parse(criteriaTokens);

			int operandVariableCount = 0;
			int typeVariableCount = 0;

			foreach (var token in tokenized.Tokens)
			{
				if (token.TokenType == TokenType.OperandVariable)
				{
					operandVariableCount = Math.Max(operandVariableCount, token.Index);
				}
				else if (token.TokenType == TokenType.TypeVariable)
				{
					typeVariableCount = Math.Max(typeVariableCount, token.Index);
				}
			}

			var tree = new TransformRule(match, criteria, transform, operandVariableCount, typeVariableCount);

			return tree;
		}

		protected Node StartParse(List<Token> tokens)
		{
			int at = 0;
			return StartParse(tokens, ref at);
		}

		protected Node StartParse(List<Token> tokens, ref int at)
		{
			var word = tokens[at];

			if (word.TokenType == TokenType.InstructionName)
			{
				return ParseInstructionNode(tokens, ref at);
			}
			else if (word.TokenType == TokenType.OpenBracket)
			{
				return ParseBracketExpression(tokens, ref at);
			}

			throw new CompilerException("ExpressionEvaluation: Invalid parse: error at " + word.Position.ToString() + " unexpected token: " + word);
		}

		protected Node Parse(List<Token> tokens, ref int at)
		{
			var word = tokens[at];

			if (word.TokenType == TokenType.InstructionName)
			{
				return ParseInstructionNode(tokens, ref at);
			}
			else if (word.TokenType == TokenType.ConstLiteral)
			{
				// the next token should be a variable
				var next = tokens[++at];

				if (next.TokenType != TokenType.OperandVariable)
					return null; // error

				var node = new Node(NodeType.ConstantVariable, next.Value, next.Index);

				// next token should be a close paran
				var next2 = tokens[++at];

				if (next2.TokenType != TokenType.CloseParens)
					return null; // error

				at++;
				return node;
			}

			throw new CompilerException("ExpressionEvaluation: Invalid parse: error at " + word.Position.ToString() + " unexpected token: " + word);
		}

		protected Node ParseInstructionNode(List<Token> tokens, ref int at)
		{
			var word = tokens[at++];

			var instruction = instructionMap[word.Value];

			var node = new Node(instruction);

			while (at < tokens.Count)
			{
				var token = tokens[at++];

				if (token.TokenType == TokenType.CloseParens)
				{
					return node;
				}

				Node parentNode = null;

				if (token.TokenType == TokenType.OpenParens)
				{
					parentNode = Parse(tokens, ref at);

					if (parentNode == null)
						return null;
				}
				else if (token.TokenType == TokenType.SignedIntegerConstant)
				{
					parentNode = new Node((long)token.Integer);
				}
				else if (token.TokenType == TokenType.UnsignedIntegerConstant)
				{
					parentNode = new Node((ulong)token.Integer);
				}
				else if (token.TokenType == TokenType.DoubleConstant)
				{
					parentNode = new Node((long)token.Double);
				}
				else if (token.TokenType == TokenType.OpenBracket)
				{
					parentNode = ParseBracketExpression(tokens, ref at);
				}
				else if (token.TokenType == TokenType.OperandVariable)
				{
					if (physicalRegisterMap.TryGetValue(token.Value, out PhysicalRegister physicalRegister))
					{
						parentNode = new Node(physicalRegister);
					}
					else if (token.Value[0] == 'v' && token.Value.Length > 1)
					{
						parentNode = new Node(NodeType.VirtualRegister, token.Value, token.Index);
					}
					else
					{
						parentNode = new Node(NodeType.OperandVariable, token.Value, token.Index);
					}
				}
				else
				{
					throw new CompilerException("ExpressionEvaluation: Invalid parse: error at " + word.Position.ToString() + " unexpected token: " + word);
				}

				node.AddNode(parentNode);
			}

			return node;
		}

		private static Node ParseBracketExpression(List<Token> tokens, ref int at)
		{
			var bracketedTokens = new List<Token>();

			for (at++; at < tokens.Count; at++)
			{
				if (tokens[at].TokenType == TokenType.CloseBracket)
				{
					break;
				}

				bracketedTokens.Add(tokens[at]);
			}

			var expressionNode = ExpressionParser.Parse(bracketedTokens);

			return new Node(expressionNode);
		}
	}
}