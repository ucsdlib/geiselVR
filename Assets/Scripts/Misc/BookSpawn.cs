using UnityEngine;

public class BookSpawn : MonoBehaviour
{
	public BookController book1;
	public BookController book2;
	public BookController book3;

	private void Start()
	{
		book1.SetMeta(new Book(
			"1", "A1", "Ender's Game", "Various authors", 12, "Science Fiction",
			"Space station fights", "A young boy becomes a military leader"));
		book2.SetMeta(new Book(
			"2", "A2", "Harry Potter: Azkaban", "JK Rowling", 20, "Fantasy",
			"Magic", "Rowling creates a magical world"));
		book3.SetMeta(new Book(
			"3", "A3", "Meditations", "Marcus Aurelius", 12, "Non-fiction", 
			"Philosophy", "Thoughts of Marcus Aurelius"));

		book1.LoadData ();
		book2.LoadData ();
		book3.LoadData ();
	}
}