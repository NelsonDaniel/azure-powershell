﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.KeyVault.Models;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.KeyVault
{
    /// <summary>
    /// Get-AzureKeyVaultCertificateContact gets the list of contacts for certificate objects in key vault.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, CmdletNoun.AzureKeyVaultCertificateContact,        
        DefaultParameterSetName = ByVaultNameParameterSet,
        HelpUri = Constants.KeyVaultHelpUri)]
    [OutputType(typeof(List<PSKeyVaultCertificateContact>))]
    public class GetAzureKeyVaultCertificateContact : KeyVaultCmdletBase
    {
        #region Parameter Set Names

        private const string ByVaultNameParameterSet = "VaultName";
        private const string ByInputObjectParameterSet = "ByInputObject";
        private const string ByResourceIdParameterSet = "ByResourceId";

        #endregion

        #region Input Parameter Definitions

        /// <summary>
        /// VaultName
        /// </summary>
        [Parameter(Mandatory = true,
                   ParameterSetName = ByVaultNameParameterSet,
                   Position = 0,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "Vault name. Cmdlet constructs the FQDN of a vault based on the name and currently selected environment.")]
        [ValidateNotNullOrEmpty]
        public string VaultName { get; set; }

        /// <summary>
        /// InputObject
        /// </summary>
        [Parameter(Mandatory = true,
                   ParameterSetName = ByInputObjectParameterSet,
                   Position = 0,
                   ValueFromPipeline = true,
                   HelpMessage = "KeyVault object.")]
        [ValidateNotNullOrEmpty]
        public PSVault InputObject { get; set; }

        /// <summary>
        /// ResourceId
        /// </summary>
        [Parameter(Mandatory = true,
                   ParameterSetName = ByResourceIdParameterSet,
                   Position = 0,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "KeyVault Id.")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        #endregion

        public override void ExecuteCmdlet()
        {
            Contacts contacts;

            if (InputObject != null)
            {
                VaultName = InputObject.VaultName.ToString();
            }
            else if (!string.IsNullOrEmpty(ResourceId))
            {
                var parsedResourceId = new ResourceIdentifier(ResourceId);
                VaultName = parsedResourceId.ResourceName;
            }

            try
            {
                contacts = this.DataServiceClient.GetCertificateContacts(this.VaultName);
            }
            catch (KeyVaultErrorException exception)
            {
                if (exception.Response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }

                contacts = null;
            }

            if (contacts == null ||
                contacts.ContactList == null)
            {
                return;
            }

            var contactsModel = new List<PSKeyVaultCertificateContact>();

            foreach (var contact in contacts.ContactList)
            {
                contactsModel.Add(PSKeyVaultCertificateContact.FromKVCertificateContact(contact, VaultName));
            }

            this.WriteObject(contactsModel, true);
        }
    }
}
