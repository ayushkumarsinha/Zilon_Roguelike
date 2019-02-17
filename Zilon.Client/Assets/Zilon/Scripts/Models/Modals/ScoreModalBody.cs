﻿using System;

using Assets.Zilon.Scripts;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Zilon.Core.Tactics;

public class ScoreModalBody : MonoBehaviour, IModalWindowHandler
{
    public Text TotalScoreText;

    public Text DetailsText;

    [Inject] IScoreManager _scoreManager;

    public string Caption => "Scores";

    public event EventHandler Closed;

    public void Init()
    {
        // TODO Сделать анимацию - плавное накручивание очков через Lerp от инта
        TotalScoreText.text = _scoreManager.BaseScores.ToString();

        DetailsText.text += "=== Your killed ===" + "\n";
        foreach (var frag in _scoreManager.Frags)
        {
            DetailsText.text += $"{frag.Key.Name?.En ?? frag.Key.Name?.Ru ?? frag.Key.ToString()}:{frag.Value}" + "\n";
        }
    }

    public void ApplyChanges()
    {
        // TODO Здесь будет рестарт игры
    }

    public void CancelChanges()
    {
        throw new NotImplementedException();
    }
}
