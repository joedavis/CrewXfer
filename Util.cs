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

        public static bool HasSpace(Part p)
        {
            return p.protoModuleCrew.Count < p.CrewCapacity;
        }

        public static bool PartsAreConnected(Part part, Part otherPart)
        {
            var spaces = CLSClient.GetCLS().Vessel.Spaces;
            var partLists = spaces.Select(space => space.Parts.Select(p => p.Part));
            return partLists.Where(p => p.Contains(part) && p.Contains(otherPart)).Count() > 0;
        }
    }
}
