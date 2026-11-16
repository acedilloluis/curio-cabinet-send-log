# Curio Cabinet Send Log

Curio Cabinet Send Log is a simple CLI tool to record climbs. It records info such as the name of the climb, its grade, whether it was sent, number of attempts, the date and a short note. It stores climb info locally as a json file. Curio Cabinet Send Log allows a user to create a new log, update a log, filter and list logs, remove a log, and generate some summary statistics.

## Usage

- Create new log with title "test" and grade "test-grade"
```
create "test" "test-grade"
```

- Edit log "test" and change note to "update note"
```
edit "test" -edn "update note"
```

- List all climbs that were sent
```
list -fs
```

- Remove log "test"
```
remove "test"
```

- Generate summary stats about climbs in log
```
stats
```