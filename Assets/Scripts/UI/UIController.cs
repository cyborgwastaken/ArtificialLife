using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace ArtificialLife
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIController : MonoBehaviour
    {
        [SerializeField] SimulationManager _sim;
        [SerializeField] StyleSheet _style;
        [SerializeField] Key _settingsKey = Key.F1;

        VisualElement _root;
        Label _gen, _time, _alive, _food, _lastGen, _life, _fit, _foodOff;
        Button _pauseBtn;
        readonly List<(Button b, float mult)> _speedBtns = new List<(Button, float)>();
        VisualElement _settings;
        bool _settingsOpen;

        void OnEnable()
        {
            if (_sim == null) { Debug.LogError("UIController: Sim not assigned."); return; }

            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.Clear();
            _root.pickingMode = PickingMode.Ignore;
            if (_style != null) _root.styleSheets.Add(_style);

            BuildHud();
            BuildSettings();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb[_settingsKey].wasPressedThisFrame) ToggleSettings();
            RefreshHud();
        }

        // ---------- HUD ----------
        void BuildHud()
        {
            var p = Panel("hud");
            p.Add(Title("Artificial Life"));

            _gen   = KV(p, "Generation");
            _time  = KV(p, "Time");
            _alive = KV(p, "Alive");
            _food  = KV(p, "Food");

            p.Add(Section("LAST GENERATION"));
            _lastGen = KV(p, "Index");
            _life    = KV(p, "Lifespan");
            _fit     = KV(p, "Fitness");
            _foodOff = KV(p, "Food / offspring");

            var r1 = Row("btnrow");
            _pauseBtn = Btn("Pause", () => _sim.Paused = !_sim.Paused);
            r1.Add(_pauseBtn);
            foreach (var m in new[] { 1f, 4f, 16f, 64f })
            {
                float mm = m;
                var b = Btn($"{mm:0}x", () => { _sim.SpeedMultiplier = mm; RefreshSpeed(); });
                _speedBtns.Add((b, mm));
                r1.Add(b);
            }
            p.Add(r1);

            var r2 = Row("btnrow");
            r2.Add(Btn("Save", () => _sim.SaveNow()));
            r2.Add(Btn("New pop", () => _sim.NewPopulation()));
            r2.Add(Btn("Settings", ToggleSettings));
            p.Add(r2);

            _root.Add(p);
            RefreshSpeed();
        }

        void RefreshHud()
        {
            if (_sim == null || _sim.Evolution == null) return;
            var evo = _sim.Evolution; var s = _sim.Settings; var st = evo.LastStats;

            _gen.text   = evo.Generation.ToString();
            _time.text  = $"{evo.GenerationElapsed:0.0} / {s.GenerationDuration:0} s";
            _alive.text = $"{CountAlive()} / {s.PopulationSize}";
            _food.text  = _sim.Environment.Food.Count.ToString();

            _lastGen.text = $"#{st.Generation}   cohort {st.Cohort}";
            _life.text    = $"avg {st.AvgLifespan:0.0}   max {st.MaxLifespan:0.0}";
            _fit.text     = $"avg {st.AvgFitness:0.0}   max {st.MaxFitness:0.0}";
            _foodOff.text = $"{st.TotalFoodConsumed}  /  {st.TotalOffspring}";
            _pauseBtn.text = _sim.Paused ? "Resume" : "Pause";
        }

        void RefreshSpeed()
        {
            foreach (var (b, m) in _speedBtns)
                b.EnableInClassList("on", Mathf.Approximately(_sim.SpeedMultiplier, m));
        }

        int CountAlive()
        {
            int n = 0; var l = _sim.Environment.Organisms;
            for (int i = 0; i < l.Count; i++) if (l[i] != null && l[i].IsAlive) n++;
            return n;
        }

        // ---------- settings ----------
        void BuildSettings()
        {
            _settings = Panel("settings");
            _settings.style.display = DisplayStyle.None;
            _settings.Add(Title("Settings  (F1)"));

            var sc = new ScrollView(ScrollViewMode.Vertical);
            sc.AddToClassList("scroll");
            _settings.Add(sc);

            var s = _sim.Settings;

            sc.Add(Section("DETERMINISM  (Seed: restart to apply)"));
            IntRow(sc, "Seed", () => s.Seed, v => s.Seed = v);
            BoolRow(sc, "Deterministic Lockstep", () => s.DeterministicLockstep, v => s.DeterministicLockstep = v);
            IntRow(sc, "Lockstep Steps/Frame", () => s.LockstepStepsPerFrame, v => s.LockstepStepsPerFrame = v);

            sc.Add(Section("WORLD / POPULATION"));
            FloatRow(sc, "World Radius", () => s.WorldRadius, v => s.WorldRadius = v);
            IntRow(sc, "Population Size", () => s.PopulationSize, v => s.PopulationSize = v);
            FloatRow(sc, "Generation Duration", () => s.GenerationDuration, v => s.GenerationDuration = v);
            BoolRow(sc, "Allow In-Life Reproduction", () => s.AllowInLifeReproduction, v => s.AllowInLifeReproduction = v);

            sc.Add(Section("FOOD"));
            IntRow(sc, "Food Target", () => s.FoodTarget, v => s.FoodTarget = v);
            FloatRow(sc, "Food Spawn / s", () => s.FoodSpawnPerSecond, v => s.FoodSpawnPerSecond = v);
            FloatRow(sc, "Food Energy", () => s.FoodEnergy, v => s.FoodEnergy = v);

            sc.Add(Section("ORGANISM BODY"));
            FloatRow(sc, "Start Energy", () => s.StartEnergy, v => s.StartEnergy = v);
            FloatRow(sc, "Max Energy", () => s.MaxEnergy, v => s.MaxEnergy = v);
            FloatRow(sc, "Move Speed Max", () => s.MoveSpeedMax, v => s.MoveSpeedMax = v);
            FloatRow(sc, "Turn Speed Max", () => s.TurnSpeedMax, v => s.TurnSpeedMax = v);
            FloatRow(sc, "Metabolism Rate", () => s.MetabolismRate, v => s.MetabolismRate = v);
            FloatRow(sc, "Move Cost Rate", () => s.MoveCostRate, v => s.MoveCostRate = v);
            FloatRow(sc, "Max Lifespan", () => s.MaxLifespan, v => s.MaxLifespan = v);
            FloatRow(sc, "Vision Range", () => s.VisionRange, v => s.VisionRange = v);
            FloatRow(sc, "Sensor Half Cone", () => s.SensorHalfConeDegrees, v => s.SensorHalfConeDegrees = v);
            FloatRow(sc, "Eat Radius", () => s.EatRadius, v => s.EatRadius = v);

            sc.Add(Section("REPRODUCTION"));
            FloatRow(sc, "Repro Threshold", () => s.ReproductionThreshold, v => s.ReproductionThreshold = v);
            FloatRow(sc, "Repro Cost", () => s.ReproductionCost, v => s.ReproductionCost = v);
            FloatRow(sc, "Offspring Start Energy", () => s.OffspringStartEnergy, v => s.OffspringStartEnergy = v);

            sc.Add(Section("MUTATION / SELECTION"));
            FloatRow(sc, "Mutation Rate", () => s.MutationRate, v => s.MutationRate = v);
            FloatRow(sc, "Mutation Strength", () => s.MutationStrength, v => s.MutationStrength = v);
            IntRow(sc, "Tournament K", () => s.TournamentK, v => s.TournamentK = v);
            IntRow(sc, "Elite Count", () => s.EliteCount, v => s.EliteCount = v);

            sc.Add(Section("FITNESS WEIGHTS"));
            FloatRow(sc, "per Second Alive", () => s.FitnessPerSecondAlive, v => s.FitnessPerSecondAlive = v);
            FloatRow(sc, "per Food", () => s.FitnessPerFood, v => s.FitnessPerFood = v);
            FloatRow(sc, "per Offspring", () => s.FitnessPerOffspring, v => s.FitnessPerOffspring = v);

            var brain = new Label($"Brain (fixed): {s.InputCount}-{s.HiddenCount}-{s.OutputCount}");
            brain.AddToClassList("k");
            brain.style.marginTop = 8;
            sc.Add(brain);

            var foot = Row("btnrow");
            foot.Add(Btn("Restart run", () => _sim.RestartRun()));
            foot.Add(Btn("Close", ToggleSettings));
            _settings.Add(foot);

            _root.Add(_settings);
        }

        void ToggleSettings()
        {
            _settingsOpen = !_settingsOpen;
            _settings.style.display = _settingsOpen ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // ---------- builders ----------
        VisualElement Panel(string cls)
        {
            var v = new VisualElement();
            v.AddToClassList("panel");
            v.AddToClassList(cls);
            return v;
        }
        Label Title(string t)   { var l = new Label(t); l.AddToClassList("title"); return l; }
        Label Section(string t) { var l = new Label(t); l.AddToClassList("section"); return l; }
        VisualElement Row(string cls) { var v = new VisualElement(); v.AddToClassList(cls); return v; }

        Label KV(VisualElement parent, string key)
        {
            var row = new VisualElement(); row.AddToClassList("row");
            var k = new Label(key); k.AddToClassList("k");
            var v = new Label("-"); v.AddToClassList("v");
            row.Add(k); row.Add(v); parent.Add(row);
            return v;
        }

        Button Btn(string text, Action onClick)
        {
            var b = new Button { text = text };
            b.clicked += onClick;
            b.AddToClassList("btn");
            return b;
        }

        void FloatRow(VisualElement parent, string label, Func<float> get, Action<float> set)
        {
            var f = new FloatField(label) { value = get() };
            f.AddToClassList("field");
            f.RegisterValueChangedCallback(e => set(e.newValue));
            parent.Add(f);
        }
        void IntRow(VisualElement parent, string label, Func<int> get, Action<int> set)
        {
            var f = new IntegerField(label) { value = get() };
            f.AddToClassList("field");
            f.RegisterValueChangedCallback(e => set(e.newValue));
            parent.Add(f);
        }
        void BoolRow(VisualElement parent, string label, Func<bool> get, Action<bool> set)
        {
            var t = new Toggle(label) { value = get() };
            t.AddToClassList("field");
            t.RegisterValueChangedCallback(e => set(e.newValue));
            parent.Add(t);
        }
    }
}
