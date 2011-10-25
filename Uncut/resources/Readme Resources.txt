Damit die Ressourcen funktionieren, folgendes beachten:

Wir benutzen im Moment 2 verschiedene Arten Ressourcen ins Programm einzubinden.

Damit die Dateien auch gefunden werden, müssen diese jeweils richtig Konfiguriert werden.
Dafür das Properties-Fenster der Ressourcen-Datei in Visual Studio öffnen (Rechtsklick auf die Datei > "Properties" (Oder "Eigenschaften" auf Deutsch nehm ich mal an...))
und dann eine von den beiden Varianten von unten einstellen.

Nummer Eins: Als "richtige" C#-Ressource die ins Programm reinkompiliert wird.
z.b. in UserInterfaceRenderer10.cs:
	using (Stream stream = assembly.GetManifestResourceStream("Uncut.Resources.UserInterface10.fx"))
Der angegebene Pfad "Uncut.Resources.UserInterface10.fx" ist nicht der Pfad zur Datei auf dem Dateisystem, sondern in welchem Ordner die Datei im Visual Studio Projekt eingefügt ist.

Die Dateien können auf diese Weise (scheinbar) nur über die Assembly-Klasse aufgerufen werden, damits klappt:
"Build Action" auf "Embedded Resource" und "Copy to Output Directory" auf "Do not Copy" steht.

	~~~

Nummer Zwei: Als extra Datei die neber dem Programm gespeichert wird.
z.b. in SimpleModel.cs:
	effect = Effect.FromFile(device, "SimpleModel10.fx", "fx_4_0");

Hier ist der Pfad auch der Pfad auf der Festplatte, damit die Datei also richtig kopiert wird nach dem Kompilieren:
"Build Action" auf "None" und "Copy to Output Directory" auf "Always" steht.