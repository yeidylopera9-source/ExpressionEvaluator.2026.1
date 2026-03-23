using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ExpressionEvaluator.Core
{
    public class Evaluator
    {
        public static double Evaluate(string infix)
        {
            var tokens = Tokenize(infix);
            var postfix = InfixToPostfix(tokens);
            return EvaluatePostfix(postfix);
        }

        private static IEnumerable<string> Tokenize(string infix)
        {
            var tokens = new List<string>();
            var numberBuffer = new StringBuilder();

            foreach (var c in infix)
            {
                if (char.IsWhiteSpace(c)) continue;

                if (char.IsDigit(c) || c == '.')
                {
                    numberBuffer.Append(c);
                }
                else if (IsOperator(c))
                {
                    if (numberBuffer.Length > 0)
                    {
                        tokens.Add(numberBuffer.ToString());
                        numberBuffer.Clear();
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    throw new Exception($"Unrecognized character: {c}");
                }
            }

            if (numberBuffer.Length > 0)
                tokens.Add(numberBuffer.ToString());

            return tokens;
        }

        private static List<string> InfixToPostfix(IEnumerable<string> tokens)
        {
            var postFix = new List<string>();
            var stack = new Stack<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _))
                {
                    postFix.Add(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        postFix.Add(stack.Pop());
                    }
                    if (stack.Count == 0) throw new Exception("Sintax error: Unbalanced parentheses.");
                    stack.Pop();
                }
                else
                {
                    while (stack.Count > 0 && stack.Peek() != "(" &&
                           PriorityStack(stack.Peek()[0]) >= PriorityInfix(token[0]))
                    {
                        postFix.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
            }

            while (stack.Count > 0)
            {
                var op = stack.Pop();
                if (op == "(") throw new Exception("Sintax error: Unbalanced parentheses.");
                postFix.Add(op);
            }

            return postFix;
        }

        private static double EvaluatePostfix(IEnumerable<string> postfix)
        {
            var stack = new Stack<double>();

            foreach (var token in postfix)
            {
                if (double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                else
                {
                    if (stack.Count < 2) throw new Exception("Evaluation error: Operands are missing..");

                    var b = stack.Pop();
                    var a = stack.Pop();

                    stack.Push(token[0] switch
                    {
                        '+' => a + b,
                        '-' => a - b,
                        '*' => a * b,
                        '/' => b != 0 ? a / b : throw new DivideByZeroException("You cannot divide by zero."),
                        '^' => Math.Pow(a, b),
                        _ => throw new Exception("Operator not supported.")
                    });
                }
            }

            return stack.Count == 1 ? stack.Pop() : throw new Exception("Syntax error in the expression.");
        }

        private static int PriorityStack(char item) => item switch
        {
            '^' => 3,
            '*' or '/' => 2,
            '+' or '-' => 1,
            '(' => 0,
            _ => throw new Exception("Priority error.")
        };

        private static int PriorityInfix(char item) => item switch
        {
            '^' => 4,
            '*' or '/' => 2,
            '+' or '-' => 1,
            '(' => 5,
            _ => throw new Exception("Priority error.")
        };

        private static bool IsOperator(char item) => "+-*/^()".Contains(item);
    }
}