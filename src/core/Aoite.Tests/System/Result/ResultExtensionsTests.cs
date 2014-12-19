﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace System
{
    public class ResultExtensionsTests
    {
        [Fact()]
        public void ThrowIfFaildedT()
        {
            Assert.Throws<ArgumentNullException>(() => ResultExtensions.ThrowIfFailded((Result)null));
        }

        [Fact()]
        public void ToFaildedT_Exception_Int32()
        {
            Assert.Throws<ArgumentNullException>(() => ResultExtensions.ToFailded((Result)null, new Exception()));
        }

        [Fact()]
        public void ToFaildedT_String_Int32()
        {
            Assert.Throws<ArgumentNullException>(() => ResultExtensions.ToFailded((Result)null, ""));
        }

        [Fact()]
        public void ToSuccessedT()
        {
            Assert.Throws<ArgumentNullException>(() => ResultExtensions.ToSuccessed((Result)null));
        }

        [Fact()]
        public void ToSuccessedT2()
        {
            Assert.Throws<ArgumentNullException>(() => ResultExtensions.ToSuccessed((Result<int>)null, 1));
        }
    }
}