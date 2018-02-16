# GeiselVR
GeiselVR is a faculty sponsored Virtual Reality interface for Geisel, the library of the University of California, San Diego. It's mission is to explore the possiblities of virtual reality for libraries and to research novel ways of interacting with collections.

The project is targeted at Oculus Rift. It is still under early development and thus may be unstable. Use at own risk.

A demonstration video can be found at [this link](https://drive.google.com/file/d/1cCNsZmEpmsADEDAzvLBtHvn7TMLVzUXR/view?usp=sharing)

![screeenshot](https://i.imgur.com/OK6L0p0.png)
A screenshot from demo video
# Running
The recommended Unity version for this project is Unity 2017.3.1; earlier versions are not guaranteed to have support. All necessary plugins are self-contained in the repository.

### Dependencies
1. **Database file**. Due to the sensitive nature of Geisel's database, it cannot be made public. The script `DBParsing/make_testdb.py` can create a sample data base file that will satisfy this dependency.

2. **Oculus runtime**

3. **Oculus Rift**

### Unity set-up
1. Run script `ParseDB/make_testdb.py` after setting desired parameters. The script is internally documented.
2. Copy generated database file to `Assets/` folder
3. Open project in Unity 2017.3.1
4. Open the scene `Scene/DML`
5. In the attribute editor of the Manager object, enter the database path relative to the `Assets/` folder.
6. Press play
