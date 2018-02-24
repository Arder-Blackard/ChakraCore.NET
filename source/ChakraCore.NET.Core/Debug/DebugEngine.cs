﻿using ChakraCore.NET.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace ChakraCore.NET.Debug
{
    public class DebugEngine
    {
        private IRuntimeDebuggingService service;
        private BlockingCollection<Action> commandQueue;
        public JavaScriptDiagStepType StepType { get; set; } = JavaScriptDiagStepType.JsDiagStepTypeContinue;
        public DebugEngine(IRuntimeDebuggingService debuggingService)
        {
            service = debuggingService;
            commandQueue = new BlockingCollection<Action>();
        }

        internal void StartProcessing()
        {
            foreach (var item in commandQueue.GetConsumingEnumerable())
            {
                item();
            }
        }

        internal void StopProcessing()
        {
            commandQueue.CompleteAdding();
        }

        public Task<BreakPoint> SetBreakpointAsync(uint scriptId, uint line, uint column)
        {
            return addCommand(() =>
            {
                return service.SetBreakpoint(scriptId, line, column);
            });
        }

        public void RemoveBreakpoint(uint breakpointId)
        {
            service.RemoveBreakpoint(breakpointId);
        }

        public void RequestAsyncBreak()
        {
            commandQueue.Add(service.RequestAsyncBreak);
        }

        public Task<SourceCode[]> GetScriptsAsync()
        {
            return addCommand(() =>
            {
                return service.GetScripts();
            });
        }

        public Task<BreakPoint[]> GetBreakPointsAsync()
        {
            return addCommand(() =>
            {
                return service.GetBreakpoints();
            });
        }

        public Task<StackTrace[]> GetStackTraceAsync()
        {
            return addCommand(() =>
            {
                return service.GetStackTrace();
            });
        }

        public Task<StackProperties> GetStackPropertiesAsync(uint stackFrameIndex)
        {
            return addCommand(() =>
            {
                return service.GetStackProperties(stackFrameIndex);
            });
        }

        public Task ClearBreakPointOnScript(uint scriptId)
        {
            return addCommand(() =>
            {
                var bps = service.GetBreakpoints();
                foreach (var item in bps.Where(x=>x.ScriptId==scriptId))
                {
                    service.RemoveBreakpoint(item.BreakpointId);
                }
            });
        }

        public Task<SourceCode> GetScriptSourceAsync(uint scriptId)
        {
            return addCommand(() =>
            {
                return service.GetScriptSource(scriptId);
            });
        }

        public Task<Variable> GetObjectFromHandleAsync(uint objectHandle)
        {
            return addCommand(() =>
            {
                return service.GetObjectFromHandle(objectHandle);
            });
        }

        public Task<VariableProperties> GetObjectPropertiesAsync(uint objectHandle, uint from = 0, uint to = 99)
        {
            return addCommand(() =>
            {
                return service.GetProperties(objectHandle, from, to);
            });
        }

        public Task<string> EvaluateAsync(string expression, uint stackFrameIndex, bool forceSetValueProp)
        {
            return addCommand(() =>
            {
                return service.Evaluate(expression, stackFrameIndex, forceSetValueProp).ToJsonString();
            });
        }

        private Task<T> addCommand<T>(Func<T> func)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            commandQueue.Add(() =>
            {
                tcs.SetResult(func());
            });
            return tcs.Task;
        }

        private Task addCommand(Action action)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            commandQueue.Add(() =>
            {
                action();
                tcs.SetResult(null);
            });
            return tcs.Task;
        }

    }
}