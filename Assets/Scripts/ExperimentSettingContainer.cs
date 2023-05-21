using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentSettingContainer {
    private List<IExperimentSetting> settings;
    private int trialAmount;
    private List<Trial> trials;

    public ExperimentSettingContainer(List<IExperimentSetting> settings, int TrialAmount) {
        this.settings = settings;
        this.trialAmount = TrialAmount;
        this.trials = new List<Trial>();
    }

    public void Populate() {
        foreach(ExperimentSetting eS in settings) {
            foreach (float timeInterval in eS.timeIntervals) {
                for (int i = 0; i < this.trialAmount; i++) {
                    Trial currentTrial = new Trial(eS.numbOfTargets, eS.numbOfDistractors, eS.depth, eS.targetsFarAway, eS.distractorsFarAway, timeInterval);
                    if (currentTrial != null) {
                        trials.Add(currentTrial);
                    }
                }
            }
        }
    }

    public List<Trial> Shuffle() {
        System.Random random = new System.Random();
        List<Trial> trialsClone = this.trials;
        List<Trial> randomizedTrials = new List<Trial>();

        for (int i = trialsClone.Count; i > 0; i--) {
            int j = random.Next(i);
            randomizedTrials.Add(trials[j]);
            trialsClone.RemoveAt(j);
        }
        return randomizedTrials;
    }
}