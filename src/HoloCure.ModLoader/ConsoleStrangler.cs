﻿using System;
using System.Threading.Tasks;

namespace HoloCure.ModLoader
{
    internal class ConsoleStrangler
    {
        protected bool Completed;
        protected volatile bool DelayPassed;
        
        private async Task Delay(int milliseconds) {
            await Task.Delay(milliseconds);

            if (!Completed) DelayPassed = true;
            Completed = true;
            await Task.CompletedTask;
        }

        private async Task WaitForInput() {
            while (!Console.KeyAvailable && !DelayPassed) { }

            Completed = true;
            await Task.CompletedTask;
        }

        public async Task Strangle(int millisecondDelay) {
#pragma warning disable CS4014
            Task.Run(() => { Delay(millisecondDelay);});
            Task.Run(() => { WaitForInput();});
#pragma warning restore CS4014

            while (!Completed) { }

            await Task.CompletedTask;
        }
    }
}