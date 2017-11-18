using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SD.FolderManagement {
    public static class FMInputManager {
        #region Controls

        [EventHandler(EventType.MouseDown, 0)]
        private static void HandleContextClicks(FolderManagerInputInfo inputInfo) {
            if (Event.current.button != 1) return;
            var contextMenu = new GenericMenu();

            if (inputInfo.FolderTreeState.SelectedItem != null)
                FillContextMenu(inputInfo, contextMenu, ContextType.Folder);
            else FillContextMenu(inputInfo, contextMenu, ContextType.FolderTree);

            contextMenu.Show(inputInfo.InputPos);
            Event.current.Use();
        }

        #endregion

        #region Setup

        private static List<KeyValuePair<EventHandlerAttribute, Delegate>> _eventHandlers;

        private static List<KeyValuePair<ContextEntryAttribute, PopupMenu.MenuFunctionData>> _contextEntries;
        private static List<KeyValuePair<ContextFillerAttribute, Delegate>> _contextFillers;

        public static void SetupInput() {
            _eventHandlers = new List<KeyValuePair<EventHandlerAttribute, Delegate>>();

            _contextEntries = new List<KeyValuePair<ContextEntryAttribute, PopupMenu.MenuFunctionData>>();
            _contextFillers = new List<KeyValuePair<ContextFillerAttribute, Delegate>>();

            var scriptAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.FullName.Contains("Assembly"));
            foreach (var assembly in scriptAssemblies)
            foreach (var type in assembly.GetTypes())
            foreach (var method in type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public |
                                                   BindingFlags.NonPublic | BindingFlags.Static)) {
                #region Event Attributes recognition and storing

                Delegate actionDelegate = null;
                foreach (var attr in method.GetCustomAttributes(true)) {
                    var attrType = attr.GetType();

                    if (attrType == typeof(EventHandlerAttribute)) {
                        if (EventHandlerAttribute.IsValid(method, attr as EventHandlerAttribute)) {
                            if (actionDelegate == null)
                                actionDelegate =
                                    Delegate.CreateDelegate(typeof(Action<FolderManagerInputInfo>), method);
                            _eventHandlers.Add(
                                new KeyValuePair<EventHandlerAttribute, Delegate>(attr as EventHandlerAttribute,
                                    actionDelegate));
                        }
                    }
                    else if (attrType == typeof(ContextEntryAttribute)) {
                        if (ContextEntryAttribute.IsValid(method, attr as ContextEntryAttribute)) {
                            if (actionDelegate == null)
                                actionDelegate =
                                    Delegate.CreateDelegate(typeof(Action<FolderManagerInputInfo>), method);

                            PopupMenu.MenuFunctionData menuFunction = callbackObj => {
                                if (!(callbackObj is FolderManagerInputInfo))
                                    throw new UnityException(
                                        "Callback Object passed by context is not of type NodeEditorMenuCallback!");
                                actionDelegate.DynamicInvoke((FolderManagerInputInfo) callbackObj);
                            };
                            _contextEntries.Add(
                                new KeyValuePair<ContextEntryAttribute, PopupMenu.MenuFunctionData>(
                                    attr as ContextEntryAttribute, menuFunction));
                        }
                    }
                    else if (attrType == typeof(ContextFillerAttribute)) {
                        if (ContextFillerAttribute.IsValid(method, attr as ContextFillerAttribute)) {
                            var methodDel =
                                Delegate.CreateDelegate(typeof(Action<FolderManagerInputInfo, GenericMenu>), method);
                            _contextFillers.Add(
                                new KeyValuePair<ContextFillerAttribute, Delegate>(attr as ContextFillerAttribute,
                                    methodDel));
                        }
                    }
                }

                #endregion
            }
            _eventHandlers.Sort((handlerA, handlerB) => handlerA.Key.Priority.CompareTo(handlerB.Key.Priority));
        }

        #endregion

        #region ContextMenu

        private static void CallEventHandlers(FolderManagerInputInfo inputInfo, bool late) {
            object[] parameter = {inputInfo};
            foreach (var eventHandler in _eventHandlers)
                if ((eventHandler.Key.HandledEvent == null ||
                     eventHandler.Key.HandledEvent == inputInfo.InputEvent.type) &&
                    (late ? eventHandler.Key.Priority >= 100 : eventHandler.Key.Priority < 100)) {
                    // Event is happening and specified priority is ok with the late-state
                    eventHandler.Value.DynamicInvoke(parameter);
                    if (inputInfo.InputEvent.type == EventType.Used)
                        return;
                }
        }

        private static void FillContextMenu(FolderManagerInputInfo inputInfo, GenericMenu contextMenu,
            ContextType contextType) {
            foreach (var contextEntry in _contextEntries)
                if (contextEntry.Key.ContextType == contextType)
                    contextMenu.AddItem(new GUIContent(contextEntry.Key.ContextPath), false, contextEntry.Value,
                        inputInfo);
            object[] fillerParams = {
                inputInfo, contextMenu
            };
            foreach (var contextFiller in _contextFillers)
                if (contextFiller.Key.ContextType == contextType) contextFiller.Value.DynamicInvoke(fillerParams);
        }

        #endregion

        #region Event Handling

        public static void HandleInputEvents(FolderTreeViewState state) {
            if (ShouldIgnoreInput())
                return;
            var inputInfo = new FolderManagerInputInfo(state);
            CallEventHandlers(inputInfo, false);
        }

        public static void HandleLateInputEvents(FolderTreeViewState state) {
            if (ShouldIgnoreInput())
                return;
            var inputInfo = new FolderManagerInputInfo(state);
            CallEventHandlers(inputInfo, true);
        }

        private static bool ShouldIgnoreInput() {
            if (PopupManager.HasPopupControl()) return true;
            return false;
        }

        #endregion
    }

    #region Attributes

    public enum ContextType {
        Folder,
        FolderTree,
        Settings
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class EventHandlerAttribute : Attribute {
        public EventHandlerAttribute(EventType handledEvent, int priority) {
            HandledEvent = handledEvent;
            Priority = priority;
        }

        public EventHandlerAttribute(int priority) {
            HandledEvent = null;
            Priority = priority;
        }

        public EventHandlerAttribute(EventType handledEvent) {
            HandledEvent = handledEvent;
            Priority = 50;
        }

        public EventType? HandledEvent { get; private set; }
        public int Priority { get; private set; }

        internal static bool IsValid(MethodInfo method, EventHandlerAttribute attribute) {
            if (!method.IsGenericMethod && !method.IsGenericMethodDefinition &&
                method.ReturnType == typeof(void)) {
                // Check if the method has the correct signature
                var methodParams = method.GetParameters();
                if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof(FolderManagerInputInfo))
                    return true;
                Debug.LogWarning("Method " + method.Name + " has incorrect signature for EventHandlerAttribute!");
            }
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ContextEntryAttribute : Attribute {
        public ContextEntryAttribute(ContextType type, string path) {
            ContextType = type;
            ContextPath = path;
        }

        public ContextType ContextType { get; private set; }
        public string ContextPath { get; private set; }

        internal static bool IsValid(MethodInfo method, ContextEntryAttribute attribute) {
            if (!method.IsGenericMethod && !method.IsGenericMethodDefinition &&
                method.ReturnType == typeof(void)) {
                // Check if the method has the correct signature
                var methodParams = method.GetParameters();
                if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof(FolderManagerInputInfo))
                    return true;
                Debug.LogWarning("Method " + method.Name + " has incorrect signature for ContextAttribute!");
            }
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ContextFillerAttribute : Attribute {
        public ContextFillerAttribute(ContextType type) {
            ContextType = type;
        }

        public ContextType ContextType { get; private set; }

        internal static bool IsValid(MethodInfo method, ContextFillerAttribute attribute) {
            if (!method.IsGenericMethod && !method.IsGenericMethodDefinition &&
                method.ReturnType == typeof(void)) {
                // Check if the method has the correct signature
                var methodParams = method.GetParameters();
                if (methodParams.Length == 2 && methodParams[0].ParameterType == typeof(FolderManagerInputInfo) &&
                    methodParams[1].ParameterType == typeof(GenericMenu))
                    return true;
                Debug.LogWarning("Method " + method.Name + " has incorrect signature for ContextAttribute!");
            }
            return false;
        }
    }

    #endregion
}