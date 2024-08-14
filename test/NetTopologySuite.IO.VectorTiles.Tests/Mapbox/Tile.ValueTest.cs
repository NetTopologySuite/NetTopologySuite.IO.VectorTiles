using NetTopologySuite.IO.VectorTiles.Mapbox;
using System;
using Xunit;

namespace NetTopologySuite.IO.VectorTiles.Tests.Mapbox
{
    public class TileValueTest
    {
        [Fact]
        public void TestHashCodeDontChangeOnInstance()
        {
            var v = new Tile.Value();
            v.StringValue = "Hello";
            int hashCode = v.GetHashCode();

            // Test value change
            v.StringValue = "World";
            Assert.Equal(hashCode, v.GetHashCode());

            // Test type change, too.
            v.BoolValue = true;
            Assert.Equal(hashCode, v.GetHashCode());
        }

        [Fact/*(Skip = "This is known to fail.")*/]
        public void TestEquals()
        {
            var v1 = new Tile.Value {StringValue = "Hello"};
            var v2 = new Tile.Value {BoolValue = true};
            Assert.False(v1.Equals(v2));

            v2 = new Tile.Value {StringValue = "Hello"};
            Assert.True(v1.Equals(v2));
        }

        [Fact]
        public void TestEqualsWithOverride()
        {
            var v1 = new Tile.Value { StringValue = "Hello" };
            var v2 = new Tile.Value { BoolValue = true };
            Assert.False(v1.Equals(v2));

            v2 = new Tile.Value { StringValue = "Hello" };
            Assert.True(v1.Equals(v2));
        }

        [Fact]
        public void TestEqualsOnDataTypes()
        {
            CheckEquals(new Tile.Value { BoolValue = true }, new Tile.Value { BoolValue = true }, true);
            CheckEquals(new Tile.Value { BoolValue = true }, new Tile.Value { BoolValue = false }, false);

            CheckEquals(new Tile.Value { DoubleValue = Math.PI }, new Tile.Value { DoubleValue = Math.PI }, true);
            CheckEquals(new Tile.Value { DoubleValue = Math.PI }, new Tile.Value { DoubleValue = -Math.PI }, false);

            CheckEquals(new Tile.Value { FloatValue = (float)Math.PI }, new Tile.Value { FloatValue = (float)Math.PI }, true);
            CheckEquals(new Tile.Value { FloatValue = (float)Math.PI }, new Tile.Value { FloatValue = -(float)Math.PI }, false);

            CheckEquals(new Tile.Value { IntValue = 3 }, new Tile.Value { IntValue = 3 }, true);
            CheckEquals(new Tile.Value { IntValue = 3 }, new Tile.Value { IntValue = -3 }, false);

            CheckEquals(new Tile.Value { UintValue = 3 }, new Tile.Value { UintValue = 3 }, true);
            CheckEquals(new Tile.Value { UintValue = 3 }, new Tile.Value { UintValue = 4 }, false);

            CheckEquals(new Tile.Value { SintValue = 6 }, new Tile.Value { SintValue = 6 }, true);
            CheckEquals(new Tile.Value { SintValue = 6 }, new Tile.Value { SintValue = 5 }, false);

            CheckEquals(new Tile.Value { StringValue = "Hello" }, new Tile.Value { StringValue = "Hello" }, true);
            CheckEquals(new Tile.Value { StringValue = "Hello" }, new Tile.Value { StringValue = "World" }, false);
        }

        private void CheckEquals(Tile.Value v1, Tile.Value v2, bool expected)
        {
            Assert.Equal(expected, v1.Equals(v2));
            Assert.Equal(expected, v2.Equals(v1));
        }
    }
}
