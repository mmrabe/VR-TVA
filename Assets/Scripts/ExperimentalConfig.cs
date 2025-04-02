using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine.Networking;
using System.Linq;


static class Extensions {

    public static System.Random rng = new System.Random();  

    public static void Shuffle<T>(this IList<T> ts) {
		var count = ts.Count;
		var last = count - 1;
		for (var i = 0; i < last; ++i) {
			var r = UnityEngine.Random.Range(i, count);
			var tmp = ts[i];
			ts[i] = ts[r];
			ts[r] = tmp;
		}
	}
}


public class Stopclock
{
    public float Duration { get; private set; }
    public int Frames { get; private set; }
    public Timeable Parent { get; private set; }
    public void Reset() => (Frames, Duration) = (0, 0f);
    public void Increment(float time)
    {
        Duration += time;
        Frames++;
    }
    public Stopclock(Timeable Parent)
    {
        this.Parent = Parent;
        this.Duration = 0f;
        this.Frames = 0;
    }
    public void Start(Experiment e) => e.EnqueueTaskOnNextFrame(delegate () { e.CurrentTimers.Add(this); });
    public void Stop(Experiment e) => e.EnqueueTaskOnNextFrame(delegate () { e.CurrentTimers.Remove(this); Parent.RegisterDuration(this); });
}

public interface Timeable
{
    public void RegisterDuration(Stopclock Clock);
}

public abstract class TVAObject
{
    [XmlAttribute]
    public float Depth;
    [XmlAttribute]
    public int Position;
    public override string ToString() => $"Object at position {Position} and depth {Depth}";
    public virtual void LogTo(Dictionary<string, object> Log, string Prefix, int Index)
    {
        Log[Prefix + "Depth" + Index] = Depth;
        Log[Prefix + "Position" + Index] = Position;
    }
    public virtual ISet<string> LogKeys(string Prefix, int Index)
    {
        HashSet<string> Keys = new HashSet<string>();
        Keys.Add(Prefix + "Depth" + Index);
        Keys.Add(Prefix + "Position" + Index);
        return Keys;
    }
    public abstract GameObject GetInstance(Experiment e);
}

public class TVACharacter : TVAObject
{
    [XmlAttribute]
    public string Color;
    [XmlAttribute]
    public string Display;
    public override string ToString() => $"Character “{Display}” of color {Color} at position {Position} and depth {Depth}";
    public override void LogTo(Dictionary<string, object> Log, string Prefix, int Index)
    {
        base.LogTo(Log, Prefix, Index);
        Log[Prefix + "Color" + Index] = Color;
        Log[Prefix + "Character" + Index] = Display;
    }
    public override ISet<string> LogKeys(string Prefix, int Index)
    {
        ISet<string> Keys = base.LogKeys(Prefix, Index);
        Keys.Add(Prefix + "Color" + Index);
        Keys.Add(Prefix + "Character" + Index);
        return Keys;
    }
    public override GameObject GetInstance(Experiment e) => GameObject.Instantiate(e.symbols[(Color, Display)]);
}


public abstract class TrialType : Timeable
{
    private Stopclock Clock;
    public string ReadableType { get => this.GetReadableType(); }
    public enum TrialState
    {
        Initialized, Ready, Started, Finished, Destroyed
    }
    public TrialType()
    {
        this.Clock = new Stopclock(this);
    }
    public void RegisterDuration(Stopclock Clock)
    {
        Log["Duration"] = Clock.Duration;
        Log["FPS"] = 1000f * Clock.Frames / Clock.Duration;
    }
    public bool IsInitialized { get => State >= TrialState.Initialized && State < TrialState.Destroyed; }
    public bool IsFinished { get => State >= TrialState.Finished; }
    public class InvalidTrialStateException : InvalidOperationException
    {
        public TrialType Trial { get; private set; }
        public TrialState[] ExpectedStates { get; private set; }
        public TrialState CurrentState { get; private set; }
        public InvalidTrialStateException(TrialType trial, TrialState[] expected) : base("Trial " + trial.Log["Trial"] + " is in state " + trial.State + " but state(s) " + String.Join("/", expected) + " was/were expected!") => (Trial, ExpectedStates, CurrentState) = (trial, expected, trial.State);
    }

    [XmlIgnore]
    public TrialState State { get; protected set; } = TrialState.Initialized;
    [XmlIgnore]
    public Dictionary<string, object> Log = new Dictionary<string, object>();
    private Experiment? _Experiment = null;
    [XmlIgnore]
    public Experiment? Experiment { get => this is ExperimentRoot ? _Experiment : ParentBlock.Experiment; protected set { _Experiment = value; } }
    [XmlIgnore]
    public BlockTrialType? ParentBlock = null;
    protected virtual string GetReadableType() => this.GetType().ToString();
    public virtual void Prepare()
    {
        AssertTrialState(TrialState.Initialized);
        Log["Type"] = GetReadableType();
        State = TrialState.Ready;
    }
    public void AssertTrialState(params TrialState[] expected)
    {
        foreach (TrialState state in expected) if (this.State == state) return;
        throw new InvalidTrialStateException(this, expected);
    }
    public virtual void Start()
    {
        Debug.Log("Started: "+ToString());
        AssertTrialState(TrialState.Ready);
        Log["TimeStarted"] = DateTime.Now;
        Clock.Start(Experiment);
        State = TrialState.Started;
        if (ParentBlock != null)
        {
            ParentBlock.TrialStarted(this);
        }
    }
    public virtual void Update()
    {
        AssertTrialState(TrialState.Started);
    }
    public virtual void Finish()
    {
        AssertTrialState(TrialState.Started);
        Log["TimeFinished"] = DateTime.Now;
        Clock.Stop(Experiment);
        State = TrialState.Finished;
        if (ParentBlock != null)
        {
            ParentBlock.TrialFinished(this);
        }
        Experiment.EnqueueTaskOnNextFrame(this.Destroy);
    }
    public virtual ISet<string> LogKeys()
    {
        var Keys = new HashSet<string> { "Trial", "Type", "TimeStarted", "TimeFinished", "Duration", "FPS" };
        foreach (string Key in Log.Keys) Keys.Add(Key);
        return Keys;
    }
    public virtual void Destroy()
    {
        if(State != TrialState.Destroyed) {
            AssertTrialState(TrialState.Finished);
            State = TrialState.Destroyed;
        }
    }
    private static string EscapeCSV(string s)
    {
        if (s.Contains(",") || s.Contains(" ") || s.Contains("\""))
        {
            if (s.Contains("\""))
            {
                s = s.Replace("\"", "\"\"");
            }
            s = $"\"{s}\"";
        }
        return s;
    }
    protected static void WriteArrayToCSV(IEnumerable<string> Keys, StreamWriter writer)
    {
        bool FirstValue = true;
        foreach (string Key in Keys)
        {
            if (FirstValue)
            {
                FirstValue = false;
            }
            else
            {
                writer.Write(",");
            }
            writer.Write(EscapeCSV(Key));
        }
        writer.WriteLine("");
    }
    protected static void WriteDictionaryToCSV(IEnumerable<string> Keys, Dictionary<string, object> Log, StreamWriter writer)
    {
        bool FirstValue = true;
        foreach (string Key in Keys)
        {
            if (FirstValue)
            {
                FirstValue = false;
            }
            else
            {
                writer.Write(",");
            }
            if (Log.ContainsKey(Key))
            {
                if (Log[Key] != null)
                {
                    string val;
                    if (Log[Key] is DateTime)
                    {
                        val = ((DateTime)Log[Key]).ToString("s");
                    }
                    else
                    {
                        val = Log[Key].ToString();
                    }
                    writer.Write(EscapeCSV(val));
                }
            }
        }
        writer.WriteLine("");
    }
    public virtual void LogCSV(IEnumerable<string> Keys, StreamWriter writer)
    {
        WriteDictionaryToCSV(Keys, Log, writer);
    }
    public virtual (float, float) CurrentProgress { get => (State >= TrialState.Finished ? 1f : 0f, 1f); }
    public virtual float CurrentProgressPercent
    {
        get
        {
            var progress = this.CurrentProgress;
            return 100f * progress.Item1 / progress.Item2;
        }
    }
}

public class InstructionTrialType : TrialType
{
    [XmlAttribute]
    public string Message;
    protected override string GetReadableType() => "Instruction";
    public override string ToString() => $"Instruction message “{Message}”";
    public override void Start()
    {
        base.Start();
        Experiment.textBox.text = Message;
        Experiment.textBox.gameObject.SetActive(true);
        Experiment.FixationCrossObject.gameObject.SetActive(false);
    }
    public override void Finish()
    {
        Experiment.textBox.gameObject.SetActive(false);
        base.Finish();
    }
    public override void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.One) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.Three) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) || Input.GetKeyUp(KeyCode.Return))
        {
            Finish();
        }
    }
}

public class BlockFeedbackTrialType : InstructionTrialType
{
    protected override string GetReadableType() => "BlockFeedback";
    public override string ToString() => $"Block feedback screen";
    public override void Start()
    {
        base.Start();
        Experiment.textBox.text = Message.Replace("[block_score]", ((int)Math.Round(ParentBlock.Score())).ToString());
        Experiment.textBox.gameObject.SetActive(true);
        Experiment.FixationCrossObject.gameObject.SetActive(false);
    }
    public override void Finish()
    {
        Experiment.textBox.gameObject.SetActive(false);
        base.Finish();
    }
    public override void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.One) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.Three) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) || Input.GetKeyUp(KeyCode.Return))
        {
            Finish();
        }
    }
}

public class TimedTrialType : TrialType
{
    public class TrialStage : Timeable
    {
        public string Name { get; set; }
        public Stopclock Clock { get; }
        public bool RequestFinish { get; protected set; } = false;
        protected TimedTrialType Trial { get; private set; }
        public TrialStage(string name)
        {
            Name = name;
            Clock = new Stopclock(this);
        }
        public void RegisterDuration(Stopclock Clock) { }
        public virtual void Update() { }
        public virtual ISet<string> LogKeys()
        {
            return new HashSet<string> { "Duration" + Name, "FPS" + Name };
        }
        public virtual void LogTo(Dictionary<string, object> Log)
        {
            Log["Duration" + Name] = Clock.Duration;
            Log["FPS" + Name] = 1000f * Clock.Frames / Clock.Duration;
        }
        public virtual void Start(TimedTrialType Trial)
        {
            this.Trial = Trial;
            Clock.Start(Trial.Experiment);
        }
        public virtual void Finish()
        {
            Clock.Stop(Trial.Experiment);
        }
    }
    public class TimedStage : TrialStage
    {
        public float DisplayDuration { get; set; }
        public TimedStage(string name, float duration) : base(name)
        {
            DisplayDuration = duration;
        }
        public override void Update()
        {
            base.Update();
            if (Trial.CurrentStage == this && Clock.Duration + 1000f / Trial.Experiment.CurrentFrameRate >= DisplayDuration)
            {
                //if(Trial.CurrentStage == this && Clock.Duration + Time.deltaTime * 1000f >= DisplayDuration) {
                RequestFinish = true;
            }
        }
        public override ISet<string> LogKeys()
        {
            ISet<string> keys = base.LogKeys();
            keys.Add("ScheduledDuration" + Name);
            return keys;
        }
        public override void LogTo(Dictionary<string, object> Log)
        {
            base.LogTo(Log);
            Log["ScheduledDuration" + Name] = DisplayDuration;
        }
    }
    public class InputStage : TrialStage
    {
        public List<OVRInput.Button> buttons;
        public List<KeyCode> keyCodes;
        public OVRInput.Button buttonPressed;
        public KeyCode keyPressed;
        public InputStage(string name, List<OVRInput.Button> buttons, List<KeyCode> keyCodes) : base(name)
        {
            this.buttons = buttons;
            this.keyCodes = keyCodes;
        }
        public override void Update()
        {
            base.Update();
            if (Trial.CurrentStage == this)
            {
                foreach (KeyCode keyCode in keyCodes)
                {
                    if (Input.GetKeyUp(keyCode))
                    {
                        RequestFinish = true;
                        keyPressed = keyCode;
                    }
                }
                foreach (OVRInput.Button button in buttons)
                {
                    if (OVRInput.GetUp(button))
                    {
                        RequestFinish = true;
                        buttonPressed = button;
                    }
                }
            }
        }
        public override ISet<string> LogKeys()
        {
            ISet<string> keys = base.LogKeys();
            keys.Add("ButtonPressed" + Name);
            keys.Add("KeyPressed" + Name);
            return keys;
        }
        public override void LogTo(Dictionary<string, object> Log)
        {
            base.LogTo(Log);
            Log["ButtonPressed" + Name] = buttonPressed;
            Log["KeyPressed" + Name] = keyPressed;
        }
    }
    public class MultiInputStage : TrialStage
    {
        public string keysPressed;
        public static char[] letters = new []{'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
        public MultiInputStage(string name) : base(name)
        {
            this.keysPressed = "";
        }
        private TouchScreenKeyboard input;
        public override void Start(TimedTrialType Trial)
        {
            base.Start(Trial);
            Debug.Log("Awaiting input now");
            Trial.Experiment.textBoxTop.gameObject.SetActive(true);
            Trial.Experiment.textBoxTop.text = ((TVAReportTrialType)Trial).ReportQueryTop;
            Trial.Experiment.textBoxBottom.gameObject.SetActive(true);
            Trial.Experiment.textBoxBottom.text = ((TVAReportTrialType)Trial).ReportQueryBottom;
            Trial.Experiment.textBox.text = "<b><size=40>?</size></b>";
            Trial.Experiment.textBox.gameObject.SetActive(true);
            Trial.Experiment.FixationCrossObject.SetActive(false);
            Trial.Experiment.array.HideStimuli();
            Trial.Experiment.array.HideFields();
            input = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.ASCIICapable, false);
        }
        public override void Finish()
        {
            base.Finish();
            Trial.Experiment.textBoxTop.gameObject.SetActive(false);
            Trial.Experiment.textBoxBottom.gameObject.SetActive(false);
            Trial.Experiment.textBox.gameObject.SetActive(false);
            Trial.Experiment.FixationCrossObject.SetActive(true);
            Trial.Experiment.array.ShowStimuli();
            Trial.Experiment.array.ShowFields();
        }
        public override void Update()
        {
            base.Update();
            if (Trial.CurrentStage == this)
            {
                bool LettersChanged = false;
                if(input.status == TouchScreenKeyboard.Status.Visible && keysPressed != input.text) {
                    keysPressed = "";
                    foreach(char letter in input.text.ToCharArray()) {
                        if(!keysPressed.Contains(letter.ToString().ToUpper())) keysPressed += letter.ToString().ToUpper();
                    }
                    input.text = keysPressed;
                    LettersChanged = true;
                }
                foreach (char letter in letters) {
                    if(Input.GetKeyUp(letter.ToString()) && !keysPressed.Contains(letter.ToString().ToUpper())) {
                        keysPressed += letter.ToString().ToUpper();
                        LettersChanged = true;
                        input.text = keysPressed;
                    }
                }
                if (Input.GetKeyUp(KeyCode.Backspace))
                {
                    keysPressed = "";
                    input.text = keysPressed;
                    LettersChanged = true;
                }
                if(LettersChanged)
                {   
                    if(keysPressed.Length > 0) {
                        Trial.Experiment.textBox.text = "<b><size=40>" + keysPressed + "</size></b>";
                    } else {
                        Trial.Experiment.textBox.text = "<b><size=40>?</size></b>";
                    }
                    Debug.Log("Letter input: " + (String.IsNullOrEmpty(keysPressed) ? "(empty)" : keysPressed));
                }
                if (input.status == TouchScreenKeyboard.Status.Done || Input.GetKeyUp(KeyCode.Return))
                {
                    RequestFinish = true;
                }
            }
        }
        public override ISet<string> LogKeys()
        {
            ISet<string> keys = base.LogKeys();
            keys.Add("KeysPressed" + Name);
            return keys;
        }
        public override void LogTo(Dictionary<string, object> Log)
        {
            base.LogTo(Log);
            Log["KeysPressed" + Name] = keysPressed;
        }
    }

    protected List<TrialStage> Stages = new List<TrialStage>();
    [XmlIgnore]
    public int CurrentStageIndex { get; private set; }
    [XmlIgnore]
    public bool HasCurrentStage { get => CurrentStageIndex >= 1 && CurrentStageIndex <= Stages.Count; }
    [XmlIgnore]
    public TrialStage? CurrentStage { get => HasCurrentStage ? Stages[CurrentStageIndex - 1] : null; }
    public override void Start()
    {
        base.Start();
        CurrentStageIndex = 1;
        if (HasCurrentStage)
        {
            CurrentStage.Start(this);
            StageStarted();
        }
    }
    public override void Update()
    {
        base.Update();
        if (State == TrialState.Started)
        {
            if (HasCurrentStage)
            {
                CurrentStage.Update();
                if (CurrentStage.RequestFinish)
                {
                    CurrentStage.Finish();
                    StageFinished();
                    CurrentStageIndex++;
                    if (HasCurrentStage)
                    {
                        CurrentStage.Start(this);
                        StageStarted();
                    }
                }
            }
            if (!HasCurrentStage)
            {
                Finish();
            }
        }
    }
    public override void Finish()
    {
        Experiment.EnqueueTaskOnNextFrame(delegate ()
        {
            foreach (var stage in Stages)
            {
                stage.LogTo(Log);
            }
        });
        base.Finish();
    }
    public virtual void StageStarted(TrialStage Stage) { }
    protected void StageStarted() => StageStarted(CurrentStage);
    public virtual void StageFinished(TrialStage Stage) { }
    protected void StageFinished() => StageFinished(CurrentStage);
    public override ISet<string> LogKeys()
    {
        ISet<string> Keys = base.LogKeys();
        foreach (TrialStage Stage in Stages) Keys.UnionWith(Stage.LogKeys());
        return Keys;
    }
    public override (float, float) CurrentProgress
    {
        get
        {
            if (IsFinished || CurrentStageIndex > Stages.Count) return (1f, 1f);
            else if (State < TrialState.Started || CurrentStageIndex < 1) return (0f, 1f);
            else return ((CurrentStageIndex - 1f) / Stages.Count, 1f);
        }
    }
}


public abstract class TVATrialType : TimedTrialType
{
    [XmlArray("Array")]
    [XmlArrayItem("Character", typeof(TVACharacter))]
    public List<TVAObject> Array;
    [XmlAttribute]
    public bool Randomize = false;
    [XmlAttribute]
    public string Stimuli = "ABDEFGHJKLMNOPRSTVXZ";
    [XmlAttribute]
    public float FixationCrossDuration { get => FixationStage.DisplayDuration; set => FixationStage.DisplayDuration = value; }
    [XmlAttribute]
    public float DisplayDuration { get => DisplayStage.DisplayDuration; set => DisplayStage.DisplayDuration = value; }
    [XmlAttribute]
    public float MaskDuration { get => MaskStage.DisplayDuration; set => MaskStage.DisplayDuration = value; }
    protected TimedStage FixationStage = new TimedStage("Fixation", 1000), DisplayStage = new TimedStage("Display", 100), MaskStage = new TimedStage("Mask", 100);
    public override string ToString()
    {
        string ret = $"{base.ToString()} with {Array.Count} display object(s), displayed for {DisplayDuration}ms:";
        for (int i = 0; i < Array.Count; i++)
        {
            ret += $"\n\tObject {i + 1}: {Array[i].ToString()}";
        }
        return ret;
    }
    protected List<GameObject> MySymbols = new List<GameObject>();
    protected CircleArray ScreenObject { get => Experiment.array; }
    public override void Prepare()
    { 
        
        if(Randomize) {
            var ItemSet = Array.OrderBy(x => Extensions.rng.Next()).Take(Array.Count).ToList();
            for (int i = 0; i < Array.Count; i++) ItemSet[i].Position = i+1;
            var StimulusSet = this.Stimuli.Select(x => x.ToString()).ToArray();
            var DrawnStimuli = StimulusSet.OrderBy(x => Extensions.rng.Next()).Take(Array.Count).ToList();
            for (int i = 0; i < Array.Count; i++) ((TVACharacter) Array[i]).Display = DrawnStimuli[i];
        }
        
        base.Prepare();
        Stages.Add(FixationStage);
        Stages.Add(DisplayStage);
        Stages.Add(MaskStage);
        for (int i = 0; i < Array.Count; i++) Array[i].LogTo(Log, "Display", i + 1);
        Log["DisplayCount"] = Array.Count;
        foreach (TVAObject obj in Array)
        {
            GameObject newObj = obj.GetInstance(Experiment);
            newObj.SetActive(false);
            MySymbols.Add(newObj);
        }
    }
    public override void Finish()
    {
        ScreenObject.HideStimuli();
        ScreenObject.HidePatternMask();
        ScreenObject.HideFields();
        ScreenObject.Clear();
        Experiment.FixationCrossObject.SetActive(false);
        base.Finish();
    }
    public override void Start()
    {
        base.Start();
        for (int i = 0; i < Array.Count; i++)
        {
            ScreenObject.PutIntoSlot(MySymbols[i], Array[i].Position, Array[i].Depth, 2.0f);
        }
        //ScreenObject.ShowFields();
        Experiment.FixationCrossObject.SetActive(true);
    }
    public override void StageStarted(TrialStage Stage)
    {
        base.StageStarted(Stage);
        if (Stage == FixationStage)
        {
            ScreenObject.HideStimuli();
            ScreenObject.HidePatternMask();
            ScreenObject.HideFields();
        }
        else if (Stage == MaskStage)
        {
            ScreenObject.ShowFields();
            ScreenObject.ShowPatternMask();
            ScreenObject.HideStimuli();
        }
        else if (Stage == DisplayStage)
        {
            ScreenObject.ShowFields();
            ScreenObject.ShowStimuli();
            ScreenObject.HidePatternMask();
        }
    }
    public override ISet<string> LogKeys()
    {
        var Keys = base.LogKeys();
        for (int i = 0; i < Array.Count; i++) Keys.UnionWith(Array[i].LogKeys("Display", i + 1));
        return Keys;
    }
}

[XmlRoot("TrialFeedback")]
public class TrialFeedback {
    [XmlAttribute]
    public string MessageTop;
    [XmlAttribute]
    public string MessageBottom;
}

public abstract class TVAReportTrialType : TVATrialType, Scorable
{
    public TrialFeedback? TrialFeedback = null;
    [XmlAttribute]
    public string ReportQueryTop;
    [XmlAttribute]
    public string ReportQueryBottom;
    protected MultiInputStage InputStage = new MultiInputStage("Input");
    public class TrialFeedbackStage : InputStage {
        public TrialFeedbackStage(string Name) : base(Name, new List<OVRInput.Button>  {OVRInput.Button.One,OVRInput.Button.Three,OVRInput.Button.PrimaryIndexTrigger,OVRInput.Button.SecondaryIndexTrigger}, new List<KeyCode> {KeyCode.Return}) {}
        public override void Start(TimedTrialType Trial)
        {
            base.Start(Trial);
            Trial.Experiment.array.ShowStimuli();
            Trial.Experiment.array.ShowFields();
            foreach(TVAObject o in ((TVAReportTrialType) Trial).Array) {
                if(o is TVACharacter && ((TVAReportTrialType) Trial).report.Contains(char.Parse(((TVACharacter) o).Display))) {
                    Trial.Experiment.array.ShowHighlight(o.Position);
                }
            }
            Trial.Experiment.textBoxTop.gameObject.SetActive(true);
            Trial.Experiment.textBoxTop.text = ((TVAReportTrialType) Trial).TrialFeedback.MessageTop.Replace("[score]", ((TVAReportTrialType)Trial).report.Intersect(((TVAReportTrialType)Trial).targets).Count().ToString()).Replace("[targets]", ((TVAReportTrialType)Trial).targets.Count().ToString());
            Trial.Experiment.textBoxBottom.gameObject.SetActive(true);
            Trial.Experiment.textBoxBottom.text = ((TVAReportTrialType) Trial).TrialFeedback.MessageBottom;
        }
        public override void Finish()
        {
            base.Finish();
            Trial.Experiment.array.HideStimuli();
            Trial.Experiment.array.HideFields();
            Trial.Experiment.array.HideHighlights();
            Trial.Experiment.textBoxTop.gameObject.SetActive(false);
            Trial.Experiment.textBoxBottom.gameObject.SetActive(false);
        }
    }
    protected TrialFeedbackStage FeedbackStage;
    public override void Prepare()
    {
        base.Prepare();
        Stages.Add(InputStage);
        targets = Array.Select(x => (TVACharacter) x).ToList().Where(x => x.Color.Equals("red")).Select(x => char.Parse(x.Display)).ToList();
        if(TrialFeedback != null) {
            FeedbackStage = new TrialFeedbackStage("Feedback");
            Stages.Add(FeedbackStage);
        }
    }
    public override void StageStarted(TrialStage Stage)
    {
        base.StageStarted(Stage);
        if (Stage == InputStage)
        {
            ScreenObject.HideStimuli();
            ScreenObject.HidePatternMask();
        }
    }

    public override void Finish()
    {
        Log["Score"] = report.Intersect(targets).Count();
        base.Finish();

    }
    List<char> report { get => InputStage.keysPressed.ToUpper().ToCharArray().ToList() ; }
    List<char> targets;

    public double Score() {
        return 100.0 * report.Intersect(targets).Count() / targets.Count;
    }

    public override ISet<string> LogKeys()
    {
        ISet<string> Keys = base.LogKeys();
        Keys.Add("Score");
        return Keys;
    }

}

public class TVAPartialReportTrialType : TVAReportTrialType
{
    protected override string GetReadableType() => "PartialReport";
}

public class TVAWholeReportTrialType : TVAReportTrialType
{
    protected override string GetReadableType() => "WholeReport";
}

public class TVAChangeDetectionTrialType : TVATrialType
{
    [XmlArray("Probe")]
    [XmlArrayItem("Character", typeof(TVACharacter))]
    public List<TVAObject> Probe;
    private List<GameObject> MyProbeSymbols = new List<GameObject>();
    protected override string GetReadableType() => "ChangeDetection";
    public override string ToString()
    {
        string ret = base.ToString();
        ret += $"\nProbe array with {Probe.Count} object(s) displayed after {DelayDuration}ms:";
        for (int i = 0; i < Probe.Count; i++)
        {
            ret += $"\n\tProbe {i + 1}: {Probe[i].ToString()}";
        }
        return ret;
    }
    protected InputStage InputStage = new InputStage("Input", new List<OVRInput.Button> { OVRInput.Button.One,OVRInput.Button.Three,OVRInput.Button.PrimaryIndexTrigger,OVRInput.Button.SecondaryIndexTrigger}, new List<KeyCode> { KeyCode.Return });
    [XmlAttribute]
    public float DelayDuration { get => DelayStage.DisplayDuration; set => DelayStage.DisplayDuration = value; }
    [XmlAttribute]
    public float CueDuration { get => CueStage.DisplayDuration; set => CueStage.DisplayDuration = value; }
    protected TimedStage DelayStage, CueStage;
    public TVAChangeDetectionTrialType() : base()
    {
        DelayStage = new TimedStage("Delay", 1000);
        CueStage = new TimedStage("Cue", 500);
    }
    public override void Prepare()
    {
        base.Prepare();
        for (int i = 0; i < Probe.Count; i++) Probe[i].LogTo(Log, "Probe", i + 1);
        Log["ProbeCount"] = Probe.Count;
        Stages.Add(DelayStage);
        Stages.Add(CueStage);
        Stages.Add(InputStage);
        foreach (TVAObject obj in Probe)
        {
            GameObject newObj = obj.GetInstance(Experiment);
            newObj.SetActive(false);
            MyProbeSymbols.Add(newObj);
        }
    }
    public override ISet<string> LogKeys()
    {
        var Keys = base.LogKeys();
        for (int i = 0; i < Probe.Count; i++) Keys.UnionWith(Probe[i].LogKeys("Probe", i + 1));
        return Keys;
    }
    public override void StageStarted(TrialStage Stage)
    {
        base.StageStarted(Stage);
        if (CurrentStage == DelayStage)
        {
            ScreenObject.HideStimuli();
            ScreenObject.HidePatternMask();
            ScreenObject.Clear();
            for (int i = 0; i < MyProbeSymbols.Count; i++)
            {
                ScreenObject.PutIntoSlot(MyProbeSymbols[i], Probe[i].Position, Probe[i].Depth, 2.0f);
            }
        }
        else if (Stage == CueStage)
        {
            ScreenObject.HideFields();
            foreach (var i in Probe) ScreenObject.ShowField(i.Position);
        }
        else if (CurrentStage == InputStage)
        {
            ScreenObject.ShowStimuli();
        }
    }
}

public interface Scorable {
    public double Score();
}


public class BlockTrialType : TrialType, Scorable
{
    [XmlArray("Procedure")]
    [XmlArrayItem("WholeReport", typeof(TVAWholeReportTrialType))]
    [XmlArrayItem("PartialReport", typeof(TVAPartialReportTrialType))]
    [XmlArrayItem("ChangeDetection", typeof(TVAChangeDetectionTrialType))]
    [XmlArrayItem("Instruction", typeof(InstructionTrialType))]
    [XmlArrayItem("BlockFeedback", typeof(BlockFeedbackTrialType))]
    [XmlArrayItem("Block", typeof(BlockTrialType))]
    public List<TrialType> Procedure;
    [XmlAttribute]
    public bool Randomize = false;
    [XmlIgnore]
    public int CurrentTrialNumber;
    protected override string GetReadableType() => "Block";
    public override string ToString()
    {
        string ret = $"Experimental block contains {Procedure.Count} trial(s):";
        for (int i = 0; i < Procedure.Count; i++)
        {
            ret += $"\n[{i + 1}] {Procedure[i].ToString()}";
        }
        return ret;
    }
    public override void Prepare()
    {
        CurrentTrialNumber = 0;
        if(this.Randomize) {
            Debug.Log("Randomize "+Log["Trial"]);
            this.Procedure.Shuffle();
        }
        base.Prepare();
        for (int i = 0; i < Procedure.Count; i++)
        {
            if (Log.ContainsKey("Trial"))
            {
                Procedure[i].Log["Trial"] = Log["Trial"] + "-" + (i + 1);
            }
            else
            {
                Procedure[i].Log["Trial"] = i + 1;
            }
            Procedure[i].ParentBlock = this;
            Procedure[i].Prepare();
        }
    }
    public override void Destroy()
    {
        foreach (TrialType i in Procedure) if(i.State != TrialState.Destroyed) i.Destroy();
        base.Destroy();
    }
    private void Forward()
    {
        if (!HasNextTrial)
        {
            CurrentTrialNumber++;
            Finish();
        }
        else
        {
            NextTrial.AssertTrialState(TrialState.Ready);
            CurrentTrialNumber++;
            CurrentTrial.Start();
            CurrentTrial.AssertTrialState(TrialState.Started);
        }
    }
    public double Score() {
        double Sum = 0.0;
        int Trials = 0;
        foreach(TrialType Trial in Procedure) if(Trial is Scorable && Trial.State >= TrialState.Finished) {
            Trials++;
            Sum += ((Scorable) Trial).Score();
        }
        return Trials > 0 ? Sum / Trials : 0.0;
    }
    public virtual void TrialFinished(TrialType Trial)
    {
        if (ParentBlock != null)
        {
            // propagate signal to parent block
            ParentBlock.TrialFinished(Trial);
        }
        // forward to next trial if any or finish block
        if(Trial == CurrentTrial) Forward();
    }
    public virtual void TrialStarted(TrialType Trial)
    {
        if (ParentBlock != null)
        {
            ParentBlock.TrialStarted(Trial);
        }
    }
    public override void Start()
    {
        base.Start();
        Forward();
    }
    public override void Finish()
    {
        base.Finish();
        CurrentTrialNumber = 0;
    }
    public override void Update()
    {
        if (HasCurrentTrial)
        {
            CurrentTrial.Update();
        }
    }
    public override ISet<string> LogKeys()
    {
        var Keys = base.LogKeys();
        foreach (TrialType i in Procedure) Keys.UnionWith(i.LogKeys());
        return Keys;
    }
    protected void PropagateLogValueToChildren(string Key, object Value, bool recursive)
    {
        foreach (TrialType i in Procedure)
        {
            i.Log[Key] = Value;
            if (i is BlockTrialType && recursive)
            {
                ((BlockTrialType)i).PropagateLogValueToChildren(Key, Value, true);
            }
        }
    }
    public override void LogCSV(IEnumerable<string> Keys, StreamWriter Writer)
    {
        base.LogCSV(Keys, Writer);
        //foreach (TrialType i in Procedure) i.LogCSV(Keys, Writer);
    }
    private bool IsValidTrialNumber(int Number) => Number >= 1 && Number <= Procedure.Count;
    public bool HasCurrentTrial { get => IsValidTrialNumber(CurrentTrialNumber); }
    public bool HasNextTrial { get => IsValidTrialNumber(CurrentTrialNumber + 1); }
    public TrialType? CurrentTrial { get => (HasCurrentTrial ? Procedure[CurrentTrialNumber - 1] : null); }
    public TrialType? NextTrial { get => (HasNextTrial ? Procedure[CurrentTrialNumber] : null); }
    public TrialType? CurrentDisplayTrial
    {
        get
        {
            if (!HasCurrentTrial) return null;
            else if (CurrentTrial is BlockTrialType) return ((BlockTrialType)CurrentTrial).CurrentDisplayTrial;
            else return CurrentTrial;
        }
    }
    public string? CurrentTrialID { get => HasCurrentTrial && CurrentTrial.Log.ContainsKey("Trial") ? CurrentTrial.Log["Trial"].ToString() : null; }
    public string? CurrentDisplayTrialID { get => CurrentDisplayTrial != null && CurrentDisplayTrial.Log.ContainsKey("Trial") ? CurrentDisplayTrial.Log["Trial"].ToString() : null; }
    public override (float, float) CurrentProgress
    {
        get
        {
            var counter = (0f, 0f);
            foreach (var Trial in Procedure)
            {
                var sub_counter = Trial.CurrentProgress;
                counter.Item1 += sub_counter.Item1;
                counter.Item2 += sub_counter.Item2;
            }
            return counter;
        }
    }
}

[XmlRoot("Participant")]
public class Participant
{
    [XmlAnyAttribute]
    public XmlAttribute[] Attributes;
}

[XmlRoot("Experiment")]
public class ExperimentRoot : BlockTrialType
{
    protected override string GetReadableType() => "Experiment";

    public static ExperimentRoot Load(TextAsset bindata, Experiment e)
    {
        XmlSerializer x = new XmlSerializer(typeof(ExperimentRoot));
        //ExperimentRoot i = (ExperimentRoot)x.Deserialize(fs);
        //Debug.Log("XML: "+bindata.text);
        ExperimentRoot i = (ExperimentRoot)x.Deserialize(new StringReader(bindata.text));
        i.Experiment = e;
        i.OutputFilePath = "Output_"+DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")+"-"+Extensions.rng.Next(1000,10000)+".csv";
        if(i.Participant.Attributes != null) foreach (XmlAttribute attr in i.Participant.Attributes)
        {
            if (attr.Name.Equals("OutputFile"))
            {
                i.OutputFilePath = attr.Value;
                break;
            }
        }
        return i;
    }
    public Participant Participant;
    private FileStream OutputFileStream;
    private StreamWriter Writer;
    private ISet<string> CSVKeys;
    public string OutputFilePath;
    public override void Prepare()
    {
        base.Prepare();
        if(Participant.Attributes != null) foreach (XmlAttribute i in Participant.Attributes)
        {
            Log[i.Name] = i.Value;
            PropagateLogValueToChildren(i.Name, i.Value, true);
        }
        string OutputPath = Path.Combine(Application.persistentDataPath,"results");
        if(!Directory.Exists(OutputPath)) Directory.CreateDirectory(OutputPath);
        OutputPath = Path.Combine(OutputPath,OutputFilePath);
        Debug.Log("Output path: "+OutputPath);
        OutputFileStream = new FileStream(OutputPath, FileMode.Create);
        Writer = new StreamWriter(OutputFileStream, new System.Text.UTF8Encoding(), 512, true);
    }
    public override void Start()
    {
        base.Start();
        CSVKeys = LogKeys();
        Experiment.EnqueueBackgroundTask(delegate () { WriteArrayToCSV(CSVKeys, Writer); });
    }
    public override void TrialFinished(TrialType Trial)
    {
        base.TrialFinished(Trial);
        Experiment.EnqueueBackgroundTask(delegate () { Trial.LogCSV(CSVKeys, Writer); Writer.Flush(); });
    }
    public override void Finish()
    {
        base.Finish();
        Writer.Flush();
        Writer.Close();
        Debug.Log("Output file has been flushed and closed. Experiment can now be quit safely.");
        Experiment.textBox.gameObject.SetActive(true);
        Experiment.textBox.text = "Thank you for participating!\r\nYou may now remove the headset.";
    }
    public override ISet<string> LogKeys()
    {
        var Keys = base.LogKeys();
        if(Participant.Attributes != null) foreach (XmlAttribute i in Participant.Attributes) Keys.Add(i.Name);
        return Keys;
    }
    public virtual void LogFullCSV(StreamWriter writer)
    {
        var Keys = LogKeys();
        WriteArrayToCSV(Keys, writer);
        LogCSV(Keys, writer);
    }
}


