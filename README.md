# QSEoK SSO with AD

This project can be used to have single sign-on from an Active Directory with QSEoK (Qlik Sense Enterprise on Kubernetes) without having the AD exposed as OIDC. The project will only work with the following OIDC passthrough installed on the QSEoK environment [https://github.com/ChristofSchwarz/qseok_oidc_helm](https://github.com/ChristofSchwarz/qseok_oidc_helm).


## Installation

Install the .Net solution on an IIS. Configure the IIS application pool identity to run under a domain account.

The IIS site need to run with Windows Authentication and Anonymous Authentication disabled

![Alt text](/images/winauth.PNG?raw=true "Windows Auth")


The solution works with following claim mapping for groups: \
groups: groups 
