/*
 * Copyright (c) 2014 Joe Davis <joe.davis512@gmail.com>
 *
 * Permission to use, copy, modify, and distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace CrewXfer
{
    public class CrewXferModule : PartModule
    {
        double updateStartTime = 0;

        public override void OnUpdate()
        {
            base.OnUpdate();

            Events.Clear();

            foreach (var crew in part.protoModuleCrew)
            {
                Events.Add(new BaseEvent(Events, "transfer" + crew.name, () =>
                {
                    Part otherPart = FindSelectedCrewModule();

                    if (!otherPart) return;
                    if (!HasSpace(otherPart))
                    {
                        Util.PostPebkac("Not enough space in module");
                        return;
                    }

                    Debug.Log("Moving " + crew.name);
                    part.RemoveCrewmember(crew);
                    otherPart.AddCrewmember(crew);
                    crew.seat.SpawnCrew(); // Not entirely sure if this is necessary

                    // Delay before spawning the crew again
                    updateStartTime = Planetarium.GetUniversalTime();
                }, new KSPEvent { guiName = "Transfer " + crew.name, guiActive = true }));
            }

            // According to Crew Manifest, we need to delay for a little bit before respawning the
            // crew.
            if (updateStartTime != 0 && Planetarium.GetUniversalTime() > (0.25 + updateStartTime))
            {
                vessel.SpawnCrew();
                GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);

                // Tell the game to redraw the right click menus
                Util.UpdateActionWindows();
                updateStartTime = 0;
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Debug.Log("CrewXfer: Added to " + part.name);
        }
            
        private Part FindSelectedCrewModule()
        {
            var rightClickedParts = Util.FindRightClickedParts();

            if (rightClickedParts.Count() == 1)
            {
                Util.PostPebkac("Select a part to transfer crew to first");
                return null;
            }
            else if (rightClickedParts.Count() > 2)
            {
                Util.PostPebkac("Too many parts selected");
                return null;
            }

            Part module = rightClickedParts.First(p => p != part);
            if (module.CrewCapacity > 0)
            {
                Debug.Log("found: " + module.name);
                return module;
            }

            return null;
            }

        private static bool HasSpace(Part p)
        {
            return p.protoModuleCrew.Count < p.CrewCapacity;
        }
    }

    class Util
    {
        public static IEnumerable<Part> FindRightClickedParts()
        {
            return GetActionWindows().Select<UIPartActionWindow,Part>(window => window.part);
        }

        public static List<UIPartActionWindow> GetActionWindows()
        {
            var controller = UIPartActionController.Instance;
            var privateFields = controller.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            var windowField = privateFields.First(f => f.FieldType == typeof(List<UIPartActionWindow>));
            return (List<UIPartActionWindow>) windowField.GetValue(controller);
        }

        public static void UpdateActionWindows()
        {
            foreach (var window in GetActionWindows())
            {
                window.displayDirty = true;
            }
        }

        public static void PostPebkac(String s)
        {
            ScreenMessages.PostScreenMessage(s, 5.0f, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}

