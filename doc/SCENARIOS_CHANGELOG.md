# Scenarios chahge log

## About chapter

The functionality provided by our company is constantly changing. Updating business logic, changing formats or data sets,
and adding new sports - leads to scenario changes. This section will help you find more information about the script updates 
we've made to match product changes.


##  Updates for [Release 19.08.2026](https://docs.beter.co/public)
* Added objects "participantStructure" and "excludedParticipants" to the time_table channel contract;
* Added object "playerProps" to the scoreboard channel contract;
* Added a scenario for a new sport discipline, Counter-Strike 2 (caseId range 3xxx):
3001 CS2 match between two teams. Covers participantStructure, excludedParticipants and playerProps,
including a player exclusion with a replacement during the match.

##  Appdates for [Release 17.09.2024](https://docs.beter.co/public)
* Added object "extra" to time_table channel for all existing scenarios;
* Removed object "comments" from the scoreboard channel for all existing scenarios;


##  Appdates for [Release 24.07.2024](https://docs.beter.co/public)
* Added new scenarios that describe interaction with different booking types.
2002 Regular Volta match between two teams|players. Booked only prematch odds ;
1022 Regular FIFA match between two teams|players. Booked only prematch odds ;
2003 Regular Volta match between two teams|players. Booked only live odds ;
1023 Regular FIFA match between two teams|players. Booked only live odds ;
* Add "regulation" object to scoreboard messages for all existing scenarios. 
