using UnityEngine;

public class BookSpawn : MonoBehaviour
{
	public BookController book1;
	public BookController book2;
	public BookController book3;

	private void Start()
	{
		book1.SetMeta (new Book ("A1", "Adventures of Finn, and something to make title longer", 20.0, "Thomas Paine", "A book about adventure, and something to make the title a lot longer"));
		book2.SetMeta (new Book ("A2", "The Game of the Seven Hundred Countries and we can put so much more on this title", 19.0, "Giovanni Grio", "Witness the epic tale of a yonug boy battling for the grand prize"));
		book3.SetMeta (new Book ("A3", "Star Wars The Force Awakens and the story of Harry Potter", 21.0, "George Lucas", "In a galaxy far far away, there are Jedi with lightsabers"));

		book1.LoadData ();
		book2.LoadData ();
		book3.LoadData ();
	}
}