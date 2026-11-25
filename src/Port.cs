using System;
using System.Collections.Generic;
using MelonLoader;

namespace CustomSkillsAPI
{
    internal static class Port
    {
        internal const string Version = "1.0.0";

        internal const string Release = "v5";

        const int RetryEvery = 60;
        const int GiveUpAfter = 3600;

        static MelonLogger.Instance log;
        static Action setup;
        static bool done;
        static bool waiting;
        static int frames;
        static string lastMissing;
        static int skills;

        internal static void Init(MelonLogger.Instance logger, Action run)
        {
            log = logger;
            setup = run;
            if (Plat.IsServer())
            {
                setup = null;
                Error("this is the " + Plat.Name + " build of CustomSkillsAPI " + Release + ", the player side only. Servers need CGNik's CustomSkillsAPI.dll. Nothing is set up");
                return;
            }
            Info(Plat.Name + " build of CGNik's CustomSkillsAPI " + Release + ", player side only");
        }

        internal static void Start(string when)
        {
            if (done || setup == null)
                return;
            string missing = Plat.Missing();
            if (missing != null)
            {
                if (missing != lastMissing)
                    Info("waiting for " + missing + " (" + when + ")");
                lastMissing = missing;
                if (!waiting)
                {
                    waiting = true;
                    MelonEvents.OnUpdate.Subscribe(Tick);
                }
                return;
            }
            done = true;
            DateTime start = DateTime.UtcNow;
            Info("setting up at " + when);
            Run("setup", setup);
            Info("ready at " + when + " in " + (int)(DateTime.UtcNow - start).TotalMilliseconds + " ms");
        }

        static void Tick()
        {
            if (done || setup == null)
                return;
            frames++;
            if (frames % RetryEvery != 0)
                return;
            if (frames >= GiveUpAfter)
            {
                done = true;
                Error("gave up after " + frames + " frames: the game never loaded " + lastMissing + ". No custom skill was added");
                return;
            }
            Start("frame " + frames);
        }

        internal static void InvokeEach(Action action, string eventName)
        {
            if (action == null)
                return;
            foreach (Delegate d in action.GetInvocationList())
            {
                try
                {
                    ((Action)d)();
                }
                catch (Exception ex)
                {
                    Error(eventName + " handler " + Who(d) + " failed: " + ex);
                }
            }
        }

        static string Who(Delegate d)
        {
            System.Reflection.MethodInfo m = d.Method;
            return m == null ? "?" : (m.DeclaringType == null ? "" : m.DeclaringType.FullName + ".") + m.Name;
        }

        internal static void Registered(ProfessionSkill skill)
        {
            skills++;
            Info("skill " + skill.Hash + " '" + skill.name + "' registered");
        }

        internal static void Report(List<ProgressionSlot> slots)
        {
            foreach (ProgressionSlot slot in slots)
            {
                ProfessionSkillTree tree = ProfessionSkillTree.GetProfessionTree(slot.Path);
                Info("slot " + slot.Hash + " '" + slot.Name + "' on " + slot.Path + ": skill " + (slot.Skill == null ? "none" : slot.Skill.name)
                    + ", cost " + slot.Cost + ", needs " + Plat.Join(slot.Dependencies) + ", indent " + slot.GetIndentLevel(tree)
                    + ", at " + Plat.Vec(slot.Position) + ", icon " + (slot.SlotIcon == null ? "none" : slot.SlotIcon.name)
                    + ", unlocks " + Plat.InheritCount(slot) + ", tree now " + tree.Slots.Count);
            }
            Info("ready: " + skills + (skills == 1 ? " skill" : " skills") + " registered, " + slots.Count + (slots.Count == 1 ? " slot" : " slots") + " added");
        }

        internal static void Run(string what, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Error(what + " failed: " + ex);
            }
        }

        internal static void Info(string message) => log?.Msg(message);

        internal static void Error(string message)
        {
            if (log != null)
                log.Error(message);
            else
                MelonLogger.Error(message);
        }
    }
}
