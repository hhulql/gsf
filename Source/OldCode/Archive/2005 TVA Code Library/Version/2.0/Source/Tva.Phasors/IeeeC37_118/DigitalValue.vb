'*******************************************************************************************************
'  DigitalValue.vb - IEEE C37.118 Digital value
'  Copyright � 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class DigitalValue

        Inherits DigitalValueBase

        Public Sub New(ByVal parent As IDataCell, ByVal DigitalDefinition As IDigitalDefinition, ByVal value As Int16)

            MyBase.New(parent, DigitalDefinition, value)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal DigitalDefinition As IDigitalDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent, DigitalDefinition, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal DigitalValue As IDigitalValue)

            MyBase.New(DigitalValue)

        End Sub

        Friend Shared Function CreateNewDigitalValue(ByVal parent As IDataCell, ByVal definition As IDigitalDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer) As IDigitalValue

            Return New DigitalValue(parent, definition, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

    End Class

End Namespace