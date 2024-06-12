## Structure of scenarios

Scripts are files that store a sequence of recorded (or in some cases generated) messages from the feed service channels.
Scripts are compiled from an array of JSON objects.
Every scenario consists of metadata and arrays of messages.
Metadata has fields: `caseId`, `version`, `description`, and `additionalInfo` (for some cases).
`caseId` - used to launch scripts, and all other fields have only an informational component.
There are two formats for the `caseId`, with a four-digit option and a one/two-digit option.
Four-digit scenarios are for specific sports disciplines, and one/two-digit ones are for system scenarios.

**CaseId**
| CaseId    | Sport    | Description                                                         |
| ----------| -------  | ------------------------------------------------------------------- |
| 1xxx      | Efootbal | FIFA matches                                                        |
| 2xxx      | Efootbal | Volta matches                                                       |
|    x      |    -     | System scenarios, can based on different sports disciplines         |


## Efootball scenarios

### Case 1001
<b>Description:</b> standart FIFA match between two players. All tradings are live.
Data transfer for four channels. Odds format - decimal. <br />
<b>Precondition:</b> Client is connected to feed <br />
<b>Steps:</b><br />
1. Match booked before start (default situation using autobooking filter);<br />
2. Match started;<br />
3. Match finished;<br />

### Case 2001
<b>Description:</b> standart Volta match between two players. All tradings are live.
Data transfer for four channels. Odds format - decimal.<br />
<b>Precondition:</b> Client is connected to feed <br />
<b>Steps:</b><br />
1. Match booked before start (default situation using autobooking filter);<br />
2. Match started;<br />
3. Match finished;<br />

### Case 1002
<b>Description:</b> FIFA match between bots. All tradings are live.
Data transfer for four channels. Odds format - decimal. <br />
<b>Precondition:</b> Client is connected to feed <br />
<b>Steps:</b><br />
1. Match booked before start (default situation using autobooking filter);<br />
2. Match started;<br />
3. Match finished;<br />

### Case 1003
**Do not run it in parallel**

<b>Description:</b> The client connected to feed after the match started.
Gained changed  scoreboard and markets in
[Connection Snapshot](https://docs.beter.co/public/feed/general-feed-info/types-of-messages#snapshots).
All tradings are live.
Data transfer for four channels. Odds format - decimal. <br />
<b>Precondition:</b> The client is not connected to the feed <br />
<b>Steps:</b><br />
1. Match booked before start (default situation using autobooking filter);<br />
2. Match started;<br />
3. Client connected to feed channels
4. The match continued until the end;<br />

### Case 1004
**Do not run it in parallel**

<b>Description:</b> The client connected to feed after the match finished (connection was absent during all match).
Got scoreboard and markets in
[Connection Snapshot](https://docs.beter.co/public/feed/general-feed-info/types-of-messages#snapshots).
Data transfer for four channels. Odds format - decimal. <br />
<b>Precondition:</b> The client is not connected to the feed <br />
<b>Steps:</b><br />
1. Match booked before start (default situation using autobooking filter);<br />
2. Match started;<br />
3. Match finished;
4. Client connected to feed channels;

### Case 1005
<b>Description:</b>Canceling the match before the start. The match has not opened markets at the time of cancellation.
The client received timetable and scoreboard according to the status of the match. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked before start;<br />
2. Match canceled;<br />

### Case 1006
<b>Description:</b> Cancellation of the match before the start. The match has open markets at the time of cancellation.
The client received the calculated markets, timetable and scoreboard according to the status of the match. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked before start;<br />
2. Match got opened markets;<br />
3. Match canceled;<br />

### Case 1007
<b>Description:</b> Deletion of match after publication.
The client received the calculated markets, timetable and scoreboard according to the status of the match<br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked before start;<br />
2. Match deleted;<br />

### Case 1008
<b>Description:</b> The match was ended due to the disqualification of the player.
The client received the calculated markets, timetable and scoreboard according to the status of the match<br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked before start;<br />
2. Match got opened markets;<br />
3. Firest player disqulified by personal reason; <br />
4. Match finished with technical result (L:W); <br />

### Case 1011
<b>Description:</b> The scenario in which most all possible
[incidents for efootball](https://docs.beter.co/public/feed/incident-feed/incidents#efootball) are collected (generated).<br />
<b>Precondition:</b> The client is connected to the feed. Match was booked and started in time.  <br />
<b>Details:</b>

<details> 
<summary>Incidents in first time;</summary>

| Incident name                 | Incident id|
| ------------                  | --------   |
|  Match Started                | 1029       |
| Match Period Started          | 1033       |
|Team To Kick Off               | 1086       |
| Dangerous Situation           | 1088       |
| Dangerous Situation Finished  | 1089       |
| Shot On Target                | 1055       |
|Add Point (0:1)                | 1031       |
| Change Score 0:1              | 1042       |
| Penalty                       | 1052       |
| Shot On Target                | 1055       |
| Add Point (1:1)               | 1031       | 
| Change Score 1:1              | 1042       |
| Penalty                       | 1052       |
| Missed Penalty                | 1064       |
| Corner                        | 1048       |
| Free Kick                     | 1050       |
| Shot on Target                | 1055       |
| Goal Kick                     | 1051       |
| Yellow Card                   | 1054       |
| Red Card                      | 1053       |
| Suspend                       | 1015       |
| Suspend Finished              | 1038       |
| Shot On Target                | 1055       |
| Add Point (2:1)               | 1031       |
| Change Score 2:1              | 1042       |
| Cancel Shot On Target         | 1062       |
| Cancel Add Point              | 1032       |
| Change Score (1:1)            | 1042       |
| Shot On Target				  | 1055       |
| Add Point (1:2)				  | 1031       |
| Change Score				  | 1042       |
| Dangerous Situation           | 1088       |
| Dangerous Situation Finished  | 1089       |
| Apply Manual Source           | 7          |
| Goal (2:2)					  | 18         |
| Apply Automatic Source        | 7          |
| Apply Manual Source           | 7          |
| Undo                          | 5          |
| Apply Automatic Source        | 7          |
| Match Period Finished         | 1034       |
| Match Break                   | 1044       |
| Match Break Finished          | 1045       |
</details>

<details> 
<summary>Incidents in second time;</summary>

| Incident name                 | Incident id|
  | ------------                  | --------   |
| Match Period Started          | 1033       |
| Apply Manual Source           | 7          |
| Goal                          | 18         |
| Undo                          | 5          |
| Redo                          | 6          |
| Apply Automatic Source        | 7          |
| Corner                        | 1048       |
| Yellow Card                   | 1054       |
| Match Period Finished         | 1034       |
| Match Finished 2:2            | 1030       |
</details>

### Case 1012
<b>Description:</b> Scenario with using only
[probability](https://docs.beter.co/public/feed/trading-feed/data-contracts-tr#probability)
parameter in  outcomes <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked; <br />
2. Match started; <br />
3. Match finished; <br />

### Case 1013
<b>Description:</b> Scenario with using
[probability](https://docs.beter.co/public/feed/trading-feed/data-contracts-tr#probability)
and odds parameters in  outcomes.  <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked; <br />
2. Match started; <br />
3. Match finished; <br />

### Case 1015
<b>Description:</b> Booking match after start. Client getting changedevents data in
[Booking Snapshot](https://docs.beter.co/public/feed/general-feed-info/types-of-messages#snapshots)  <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match started; <br />
2. Match booked; <br />
3. Event markets become open; <br />
4. Match finished; <br />

### Case 1016
<b>Description:</b> Scenarios with changing the result of the half and the game.  <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked;  <br />
2. The first time started; <br />
3. The first time ended with a score 1:2; <br />
4. Operator in manual mode returns to active first half; <br />
5. Operator changes the score and finishes the first time with a score 2:2; <br />
6. Second time started; <br />
7. The match ended with a score 2:3; <br />
8. Operator in manual mode returns to active second half; <br />
9. The operator changes the score and finishes the match with a score 4:3;<br />

### Case 1017
<b>Description:</b> Undo operations of different event stages. Cancel: match start, finish of the first and second half.  <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. The match booked; <br />
2. The match is started in scout tools; <br />
3. Canceling the start of the match; <br />
4. Start of the first half; <br />
5. End of the first half; <br />
6. Start of the second half; <br />
7. Cancellation of the start of the second half and transition to the first half; <br />
8. End of the first half; <br />
9. Start of the second half; <br />
10. The match is over; <br />
11. Canceling the end of the half; <br />
12. Change of score; <br />
13. Completion of the match"; <br />

### Case 1018
<b>Description:</b> Sending unexpected incident for FIFA match. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. The match booked; <br />
2. The match is started; <br />
3. During the match in incident channel sends message with `type: 1091`; <br />
4. Match finished; <br />


### Case 1019
<b>Description:</b> Sending unexpected market for FIFA match. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. The match booked; <br />
2. The match is started; <br />
3. During the match in trading  channel sends message with  `id: [78,1,1,0,[]]`; <br />
4. Match finished; <br />

### Case 1020
<b>Description:</b> Changing markets list during the match. This affects the list of markets that are transferred to the trading channel. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. The match booked; <br />
2. The match is started; <br />
3. During first time from market setting disabled three markets [Total, Exact Score, Odd/Even]; <br />
4. After start the second time restored [Total] market; <br />
5. Match finished; <br />

### Case 1021
<b>Description:</b> Changing odds format during the match. This affects the list of prices that are transferred to the trading channel. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. The match booked; <br />
2. The match is started; <br />
3. During the first half, all values except for "Decimal odds" were turned off; <br />
4. After start the second time restored "American odds"; <br />
5. Match finished; <br />


## System scenarios
### Case 1
<b>Description:</b> Generated situation in which the client receives a message with an inappropriate `id` of the event. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked; <br />
2. Match started; <br />
3. In trading channel send update with another `id: bd2ffddc-fbe4-4f90-b6f5-ef6a2bfbb045`; <br />
4. Match finished; <br />

### Case 2
**Do not run it in parallel**

<b>Description:</b> Generated situation in which the client receives older data after reconnect. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked; <br />
2. Match started; <br />
3. Client reconnected; <br />
4. The client got messages with old offset (less than was in update message before disconnecting `13001930`); <br />
5. Match finished; <br />

### Case 3
**Do not run it in parallel**

<b>Description:</b> Generated situation in which the client receives two messages with the same offset. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked; <br />
2. Match started; <br />
3. The client got two messages with same offset `13001905`
4. Match finished; <br />

### Case 4
**Do not run it in parallel**

<b>Description:</b> Generated situation in which the client receives double of messages with the same information (only different offset). <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked; <br />
2. Match started; <br />
3. The client got two messages with the same content, only offset is different `13000971` and `13000972`; <br />
4. Match finished; <br />

### Case 5
<b>Description:</b> Generated a situation in which the client stopped receiving heartbeats during the match.
After stopping sending heartbeats data sending continues. <br />
<b>Precondition:</b> The client is connected to the feed <br />
<b>Steps:</b>
1. Match booked; <br />
2. Match started; <br />
3. During the match, we stop sending heartbeats for 20 sec; <br />
4. Start sending heartbeat after 20 sec; <br />
