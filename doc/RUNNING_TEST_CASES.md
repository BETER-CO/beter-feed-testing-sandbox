# Running Test Cases

**Feed Generator** application provides REST API to start and stop test case playback. It supports the following features:
1. **Parallelizable execution**. You may run the same test case or different test cases in parallel.
2. **Test case immutability**. Each test case has it's own version. If version remains the same it means no changes
were made to the logic of the test case or data contracts. But if version increased, we specify the essence of changes
in test case description. **Note.** To execute previous version of test case use older versions of the apps.
3. **Application versions immutability**. We don't make any changes to test cases and the code without publishing of
the new version if the applications. No dynamic loading.
4. **Emulation of broken connections**. Feed Emulator by your request may stop sending heartbeats or drop the connection.
This helps to test abnormal connectivity situations.
5. **Time acceleration**. You may run a test scenario with the regular execution speed, but you may to accelerate the
playback.

Below you may find an extra details.

## Immutability and mutability of the Data
