using UnityEngine;

public class BookSpawn : MonoBehaviour
{
	public BookController book1;
	public BookController book2;
	public BookController book3;

	private void Start()
	{
		book1.SetMeta (new Book ("A1", "Adventures of Finn", 20.0, "Thomas Paine", "A book about adventure"));
		book2.SetMeta (new Book ("A2", "The Game", 19.0, "Giovanni Grio", "Witness the epic tale of a yonug boy battling for the grand prize"));
		book3.SetMeta (new Book ("A3", "Star Wars", 21.0, "George Lucas", "In a galaxy far far away, there are Jedi with lightsabers"));

		book1.LoadData ();
		book2.LoadData ();
		book3.LoadData ();
	}
}