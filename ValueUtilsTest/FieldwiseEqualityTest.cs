﻿using System;
using ExpressionToCodeLib;
using ValueUtils;
using Xunit;

namespace ValueUtilsTest
{
    public class FieldwiseEqualityTest
    {
        static readonly Func<SampleStruct, SampleStruct, bool> eq = FieldwiseEquality<SampleStruct>.Instance;

        [Fact]
        public void IdenticalValuesAreEqual() => PAssert.That(() =>
            eq(new SampleStruct(1, 2, "3", 4), new SampleStruct(1, 2, "3", 4))
        );

        [Fact]
        public void CanCheckEqualityWithNull()
        {
            PAssert.That(() => !FieldwiseEquality.AreEqual(Tuple.Create(1), null));
            PAssert.That(() => !FieldwiseEquality.AreEqual(null, Tuple.Create(1)));
            PAssert.That(() => FieldwiseEquality.AreEqual<SampleStruct?>(null, null));
            PAssert.That(() => FieldwiseEquality.AreEqual<SampleClass>(null, null));
        }


        [Fact]
        public void OneChangedMemberCausesInequality()
        {
            PAssert.That(() => !eq(new SampleStruct(1, 2, "3", 4), new SampleStruct(11, 2, "3", 4)));
            PAssert.That(() => !eq(new SampleStruct(1, 2, "3", 4), new SampleStruct(1, 12, "3", 4)));
            PAssert.That(() => !eq(new SampleStruct(1, 2, "3", 4), new SampleStruct(1, 2, "13", 4)));
            PAssert.That(() => !eq(new SampleStruct(1, 2, "3", 4), new SampleStruct(1, 2, "3", 14)));
        }

        [Fact]
        public void TuplesWithTheSameFieldValuesAreEqual() => PAssert.That(() =>
            FieldwiseEquality.AreEqual(Tuple.Create(1, 2, "3", 4), Tuple.Create(1, 2, "3", 4))
        );

        [Fact]
        public void OneDifferentObjectMemberCausesInequality()
        {
            PAssert.That(() => !FieldwiseEquality.AreEqual(Tuple.Create(1, 2, "3", 4), Tuple.Create(11, 2, "3", 4)));
            PAssert.That(() => !FieldwiseEquality.AreEqual(Tuple.Create(1, 2, "3", 4), Tuple.Create(1, 12, "3", 4)));
            PAssert.That(() => !FieldwiseEquality.AreEqual(Tuple.Create(1, 2, "3", 4), Tuple.Create(1, 2, "13", 4)));
            PAssert.That(() => !FieldwiseEquality.AreEqual(Tuple.Create(1, 2, "3", 4), Tuple.Create(1, 2, "3", 14)));
        }

        [Fact]
        public void AutoPropsAffectEquality()
        {
            PAssert.That(() => FieldwiseEquality.AreEqual(new SampleClass { AutoPropWithPrivateBackingField = "x" }, new SampleClass { AutoPropWithPrivateBackingField = "x" }));
            PAssert.That(() => !FieldwiseEquality.AreEqual(new SampleClass { AutoPropWithPrivateBackingField = "x" }, new SampleClass { AutoPropWithPrivateBackingField = "y" }));
        }

        [Fact]
        public void StructFieldsAffectEquality()
        {
            //MemberMemberBindings are buggy, so use PlainStruct=new... syntax; ref https://github.com/dotnet/corefx/issues/37968
            PAssert.That(() => FieldwiseEquality.AreEqual(new SampleClass { PlainStruct = new CustomStruct { Bla = 1 } }, new SampleClass { PlainStruct = new CustomStruct { Bla = 1 } }));
            PAssert.That(() => !FieldwiseEquality.AreEqual(new SampleClass { PlainStruct = new CustomStruct { Bla = 1 } }, new SampleClass { PlainStruct = new CustomStruct { Bla = 2 } }));
        }

        [Fact]
        public void TypeDoesNotAffectRuntimeEquality()
        {
            var sampleClass = new SampleClass { AnEnum = SampleEnum.Q };
            var sampleSubClass = new SampleSubClass { AnEnum = SampleEnum.Q };

            //This is really pretty unwanted behavior
            PAssert.That(() => FieldwiseEquality.AreEqual(sampleClass, sampleSubClass));
        }

        [Fact]
        public void SubClassesVerifyEqualityOfBaseClassFields()
        {
            PAssert.That(() => !FieldwiseEquality.AreEqual(new SampleSubClassWithFields { AnEnum = SampleEnum.Q }, new SampleSubClassWithFields { AnEnum = SampleEnum.P }));
            PAssert.That(() => FieldwiseEquality.AreEqual(new SampleSubClassWithFields { AnEnum = SampleEnum.Q }, new SampleSubClassWithFields { AnEnum = SampleEnum.Q }));
        }

        [Fact]
        public void StructIntFieldsAffectEquality()
        {
            var customStruct1 = new CustomStruct { Bla = 1 };
            var customStruct2 = new CustomStruct { Bla = 2 };
            PAssert.That(() => !FieldwiseEquality.AreEqual(customStruct1, customStruct2));
            PAssert.That(() => FieldwiseEquality.AreEqual(new CustomStruct { Bla = 3 }, new CustomStruct { Bla = 3 }));
        }

        [Fact]
        public void ClassNullableIntFieldsAffectEquality()
        {
            var object1 = new SampleClass { NullableField = null };
            var object2 = new SampleClass { NullableField = 1 };
            PAssert.That(() => !FieldwiseEquality.AreEqual(object1, object2));
            PAssert.That(() => FieldwiseEquality.AreEqual(new SampleClass { NullableField = 3 }, new SampleClass { NullableField = 3 }));
        }


        [Fact]
        public void ClassStructFieldsAffectEquality()
        {
            var object1 = new SampleClass { PlainStruct = new CustomStruct { Bla = 1 } };
            var object2 = new SampleClass { PlainStruct = new CustomStruct { Bla = 2 } };
            var object3A = new SampleClass { PlainStruct = new CustomStruct { Bla = 3 } };
            var object3B = new SampleClass { PlainStruct = new CustomStruct { Bla = 3 } };
            PAssert.That(() => !FieldwiseEquality.AreEqual(object1, object2));
            PAssert.That(() => FieldwiseEquality.AreEqual(object3A, object3B));
        }


        [Fact]
        public void ClassNullableStructFieldsAffectEquality()
        {
            var object1 = new SampleClass { NullableStruct = null };
            var object2 = new SampleClass { NullableStruct = new CustomStruct { Bla = 2 } };
            var object3A = new SampleClass { NullableStruct = new CustomStruct { Bla = 3 } };
            var object3B = new SampleClass { NullableStruct = new CustomStruct { Bla = 3 } };
            PAssert.That(() => !FieldwiseEquality.AreEqual(object1, object2));
            PAssert.That(() => FieldwiseEquality.AreEqual(object3A, object3B));
        }
    }
}
