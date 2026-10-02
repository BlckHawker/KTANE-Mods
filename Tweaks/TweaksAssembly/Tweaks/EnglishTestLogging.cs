//Question X/Y
//The earthquake _____ shook us up.
//Given answers: literally, (not literally)
//Expected answer: literally
//You chose "XXX". This is (correct./incorrect. Strike!)
//(Moving to question X/Resetting module/Module Solved). 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

internal class EnglishTestLogging : ModuleLogging
{
	private bool moduleSolved = false;

	private KMSelectable SubmitSelectable;

	private int stage = -1;
	private int totalStages;
	private static FieldInfo currentQuestionFld;
	public static FieldInfo QuestionTextFld;
	public static FieldInfo AnswerTextIndexFld;
	public static FieldInfo AnswersFld;
	public static FieldInfo CorrectAnswerIndexFld;

	private string QuestionText;
	private int AnswerTextIndex;
	private List<string> Answers;
	private int CorrectAnswerIndex;

	public EnglishTestLogging(BombComponent bombComponent) : base(bombComponent, "EnglishTestModule", "English Test")
	{
		totalStages = GetTargetQuestions();
		SubmitSelectable.OnInteract += () =>
		{

			int selectedAnswerIndex = GetSelectedAnswerIndex();

			bool correct = selectedAnswerIndex == CorrectAnswerIndex;

			Log($"You chose: {Answers[selectedAnswerIndex]}. This is {(correct ? "correct." : "incorrect. Strike!")}");

			if (correct)
			{
				Log(stage + 1 == totalStages ? "Module Solved." : $"Moving to question {stage + 1}.");
			}

			else
			{
				Log("Resetting module.");
			}

			return false;
		};

		bombComponent.OnPass += _ =>
		{
			moduleSolved = true;
			return false;
		};
	}

	private IEnumerator HandleLogging()
	{
		int targetQuestions = GetTargetQuestions();

		//Check if module has activated
		while (!ModuleActivated())
		{
			yield return new WaitForSeconds(0.1f);
		}

		while (!moduleSolved)
		{
			int oldStage = stage;
			stage = GetSolvedQuestions();
			yield return new WaitForSeconds(0.05f);
			if (stage != oldStage)
			{
				int questionNumber = GetQuestionNumber();
				Log($"Question {questionNumber}/{targetQuestions}");
				UpdateCurrentQuestionFields();
				Log(QuestionText);
				Log("Given answers: " + string.Join(", ", Answers.ToArray()));
				Log("Expected answer: " + Answers[CorrectAnswerIndex]);
			}	
		}
		yield return null;
	}

	private bool ModuleActivated() { return component.GetValue<bool>("activated"); }
	private int GetQuestionNumber() { return GetSolvedQuestions() + 1; }
	private int GetSolvedQuestions() { return component.GetValue<int>("solvedQuestions"); }
	private int GetTargetQuestions() { return component.GetValue<int>("targetQuestions"); }
	private int GetSelectedAnswerIndex() { return component.GetValue<int>("selectedAnswerIndex"); }

	private void UpdateCurrentQuestionFields()
	{
		currentQuestionFld = componentType?.GetField("currentQuestion", BindingFlags.NonPublic | BindingFlags.Instance);

		Type questionType = currentQuestionFld.GetType();

		QuestionTextFld = questionType.GetField("QuestionText", BindingFlags.Public | BindingFlags.Instance);
		AnswerTextIndexFld = questionType.GetField("AnswerTextIndex", BindingFlags.Public | BindingFlags.Instance);
		AnswersFld = questionType.GetField("Answers", BindingFlags.Public | BindingFlags.Instance);
		CorrectAnswerIndexFld = questionType.GetField("CorrectAnswerIndex", BindingFlags.Public | BindingFlags.Instance);

		QuestionText = (string) QuestionTextFld.GetValue(currentQuestionFld);
		AnswerTextIndex = (int) AnswerTextIndexFld.GetValue(currentQuestionFld);
		Answers = (List<string>) AnswersFld.GetValue(currentQuestionFld);
		CorrectAnswerIndex = (int) CorrectAnswerIndexFld.GetValue(currentQuestionFld);
	}
		
}