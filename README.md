# OSConsTools

This repository contains OneStream workspace/business-rule source plus a starter .NET MCP server for API discovery and repo-aware examples.

- Existing workspace compile project: `OSConsTools.csproj`
- MCP server project: `tools/OSConsTools.McpServer/OSConsTools.McpServer.csproj`
- MCP server docs: `tools/OSConsTools.McpServer/README.md`

## USCG dashboard VB structure

The current USCG example under `/home/runner/work/OSConsTools/OSConsTools/OS Consultant Tools/USCG/10 BUDFM/Assembly/BUDFM_Assembly` is already split into the main business-rule layers you need for a shared dashboard shell with appn-specific content:

- `Factory/BUDFM_Svc_Factory.vb` wires OneStream service types to your VB rule classes.
- `DB Extenders/BUDFM_SolutionHelper.vb` handles dashboard events and load-time routing.
- `XFBRs/BUDFM_StringHelper.vb` handles lightweight UI decisions such as mode switching and embedded dashboard name selection.
- `Helper Classes/BUDFM_AttributeSupport.vb` centralizes dashboard parameter routing and attribute loading.
- `Helper Classes/BUDFM_RP_Utilities.vb` holds reusable RP/appn/domain logic.
- `DB DataSets/BUDFM_DataSet.vb` is the place for dashboard datasets.

If you want one core dashboard with dynamic appn sections, keep the XML dashboards mostly shared and structure the VB like this:

1. Put all common dashboard routing in one helper/module like `BUDFM_AttributeSupport.vb`.
   - The existing `SetRPContentRoutingVars` method already follows the right pattern.
   - It takes `APPN_Content`/`rpAppr` and sets shared parameter names such as `prm_Mode_<APPN>`, `prm_Content_<APPN>`, `prm_Content_Page_<APPN>`, and `prm_Content_Frame_<APPN>`.
   - It also documents the naming convention `<APPN>_RP_Content`, `<APPN>_RP_Page1`, and `<APPN>_RP_Frame`.

2. Keep the shell dashboard behavior in the dashboard extender.
   - `BUDFM_SolutionHelper.vb` should stay responsible for `LoadWFDashboard`, header clicks, selection changes, and navigation.
   - When a user changes appn or RP context, call the central routing helper instead of hard-coding dashboard names all over the extender.

3. Keep simple dynamic UI decisions in the string helper.
   - `BUDFM_StringHelper.vb` is the right place for logic like `GetModeDashboard`, `ResolveRPMode`, and `RPControlState`.
   - Use it to choose between shared embedded dashboards or mode-specific variants without moving that logic into the XML.

4. Keep appn-specific business rules in small helper methods, not separate copies of the whole dashboard rule.
   - Reuse the shared extender/string/helper files.
   - Only branch where behavior actually differs by appn, ideally by passing the appn code (`OS`, `BS`, `RD`, etc.) into shared methods.
   - If a section becomes truly unique, add a focused helper file such as `Helper Classes/BUDFM_<APPN>_Dashboard.vb` rather than cloning `BUDFM_SolutionHelper.vb`.

In practice, that means:

- one shared core dashboard shell in XML,
- one shared VB routing layer that maps appn -> content/frame/page parameters,
- optional appn-specific helper modules only for the parts that truly differ.

That keeps the dashboard setup maintainable while still letting each appn swap in different embedded content.