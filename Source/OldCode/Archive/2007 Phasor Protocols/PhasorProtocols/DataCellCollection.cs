//*******************************************************************************************************
//  DataCellCollection.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent collection of <see cref="IDataCell"/> objects.
    /// </summary>
    [Serializable()]
    public class DataCellCollection : ChannelCellCollectionBase<IDataCell>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCellCollection"/> from specified parameters.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <param name="constantCellLength">Sets flag that determines if the lengths of <see cref="IDataCell"/> elements in this <see cref="DataCellCollection"/> are constant.</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="Int16.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        public DataCellCollection(int lastValidIndex, bool constantCellLength)
            : base(lastValidIndex, constantCellLength)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DataCellCollection"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataCellCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}