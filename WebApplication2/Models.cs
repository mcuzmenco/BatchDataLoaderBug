using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.Resolvers;

namespace WebApplication2
{
	public class Query
	{
		int _requestCount;

		public async Task<Book> Book(int id)
		{
			await Task.Delay(50);
			return new Book { Title = "C# in depth", Author = "Jon Skeet", Id = id };
		}

		public async Task<IReadOnlyList<Book>> GetBooks(IResolverContext context)
		{
			_requestCount = 0;
			var ids = Enumerable.Range(0, 100).ToImmutableList();

			await Task.Delay(1);
			var books = await context.BatchDataLoader(
				async (IReadOnlyList<int> bookIds, CancellationToken t) => {
					Interlocked.Increment(ref _requestCount);
					if (_requestCount > 1)
					{
						throw new Exception("Nope!");
					}

					var data = bookIds.Select(
						x => new Book
						{
							Title = "C# in depth",
							Author = "Jon Skeet",
							Id = x
						}
					);

					IReadOnlyDictionary<int, Book> result = data.ToDictionary(x => x.Id);
					await Task.Delay(1);
					return result;
				}).LoadAsync(ids, context.RequestAborted);

			return books.ToList();
		}
	}


	public class Book
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Author { get; set; }
	}
}
